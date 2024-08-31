using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Whisper;
using Whisper.Utils;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System;
using Whisper.Native;
using System.Linq;
using UnityEngine.UI;
using TMPro;
using Google.Protobuf.Protocol;

public class RealtimeSTTManager : MonoBehaviour
{
    private class Command
    {
        public List<string> keywords;
        public Action thenDo;
    }

    private Command CurCommand { get; set; }

    [Range(0f, 1f)]
    [Tooltip("How similar must the user's transcription be to trigger a match?")]
    public float SimilarityThreshold;

    [SerializeField]
    [Tooltip("Path to model weights file")]
    private string modelPath = "Whisper/ggml-small.bin";

    [SerializeField]
    [Tooltip("Determines whether the StreamingAssets folder should be prepended to the model path")]
    private bool isModelPathInStreamingAssets = true;

    [SerializeField]
    [Tooltip("Should model weights be loaded on awake?")]
    private bool initOnAwake = true;

    [SerializeField]
    [Header("Language")]
    [Tooltip("Output text language. Use empty or \"auto\" for auto-detection.")]
    public string language = "ko";

    [Tooltip("Force output text to English translation. Improves translation quality.")]
    public bool translateToEnglish;

    [Header("Advanced settings")]
    [SerializeField]
    private WhisperSamplingStrategy strategy = WhisperSamplingStrategy.WHISPER_SAMPLING_BEAM_SEARCH;

    [Tooltip("Do not use past transcription (if any) as initial prompt for the decoder.")]
    public bool noContext = false;

    [Tooltip("Force single segment output (useful for streaming).")]
    public bool singleSegment;

    [Tooltip("Output tokens with their confidence in each segment.")]
    public bool enableTokens;

    [Tooltip("Initial prompt as a string variable. " +
             "It should improve transcription quality or guide it to the right direction.")]
    [TextArea]
    public string initialPrompt;

    [Header("Experimental settings")]
    [Tooltip("[EXPERIMENTAL] Output timestamps for each token. Need enabled tokens to work.")]
    public bool tokensTimestamps;

    [Tooltip("[EXPERIMENTAL] Speed-up the audio by 2x using Phase Vocoder. " +
                 "These can significantly reduce the quality of the output.")]
    public bool speedUp = false;

    [Tooltip("[EXPERIMENTAL] Overwrite the audio context size (0 = use default). " +
                 "These can significantly reduce the quality of the output.")]
    public int audioCtx;

    public LoopingMicrophone Microphone;

    private WhisperWrapper _whisper;
    private WhisperParams _params;

    private Task<WhisperResult> _task;
    public bool AITasking
    {
        get {
            if (_task != null && !_task.IsCompleted)
            {
                return true;
            }
            else
                return false;
        }
    }

    TMP_Text _checkInferencing;
    public TMP_Text CheckInferencing
    {
        get
        {
            if (_checkInferencing == null)
                _checkInferencing = GameObject.Find("CheckInferencing").GetComponent<TMP_Text>();

            return _checkInferencing;
        }
    }

    public void ThenDo()
    {
        if(String.IsNullOrEmpty(Managers.Scenario.CurrentScenarioInfo.Place))
            Managers.Scenario.PassSpeech = true;
        else
        {
            if (Managers.Scenario.CurrentScenarioInfo.Place == Managers.Object.MyPlayer.Place)
                Managers.Scenario.PassSpeech = true;
        }

    }

    public void RegisterCommand(string command, bool player)
    {
        initializeCommands();
        
        if (player)
        {
            CurCommand.keywords = Managers.Scenario.CurrentScenarioInfo.Keywords;
            CurCommand.thenDo = ThenDo;
        }
    }

    private void initializeCommands()
    {
        if (CurCommand == null)
        {
            CurCommand = new Command();
        }
        CurCommand.keywords = null;
        CurCommand.thenDo = null;
    }

    public bool IsModelPathInStreamingAssets
    {
        get => isModelPathInStreamingAssets;
        set
        {
            if (IsLoaded || IsLoading)
            {
                throw new InvalidOperationException("Cannot change model path after loading the model");
            }

            isModelPathInStreamingAssets = value;
        }
    }

    /// <summary>
    /// Checks if whisper weights are loaded and ready to be used.
    /// </summary>
    public bool IsLoaded => _whisper != null;

    /// <summary>
    /// Checks if whisper weights are still loading and not ready.
    /// </summary>
    public bool IsLoading { get; private set; }

    private async void Awake()
    {
        if (!initOnAwake)
            return;

        await InitModel();
        
        if(Microphone == null)
            Microphone = GetComponent<LoopingMicrophone>();

        if (Microphone != null)
        {
            Microphone.OnEvaluate += Evaluate;
            Microphone.OnRecordStart += MicrophoneOnRecordStart;
            Microphone.OnRecordStop += MicrophoneOnRecordStop;
        }

        initializeCommands();
    }

    /// <summary>
    /// Load model and default parameters. Prepare it for text transcription.
    /// </summary>
    public async Task InitModel()
    {
        // check if model is already loaded or actively loading
        if (IsLoaded)
        {
            Debug.LogWarning("Whisper model is already loaded and ready for use!");
            return;
        }

        if (IsLoading)
        {
            Debug.LogWarning("Whisper model is already loading!");
            return;
        }

        // load model and default params
        IsLoading = true;
        try
        {
            var path = IsModelPathInStreamingAssets
                ? Application.streamingAssetsPath + "/" + modelPath
                : modelPath;
            _whisper = await WhisperWrapper.InitFromFileAsync(path);
            _params = WhisperParams.GetDefaultParams(strategy);
            UpdateParams();
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }

        IsLoading = false;
    }

    private void UpdateParams()
    {
        _params.Language = language;
        _params.Translate = translateToEnglish;
        _params.NoContext = noContext;
        _params.SingleSegment = singleSegment;
        _params.SpeedUp = speedUp;
        _params.AudioCtx = audioCtx;
        _params.EnableTokens = enableTokens;
        _params.TokenTimestamps = tokensTimestamps;
        _params.InitialPrompt = initialPrompt;
    }

    private void MicrophoneOnRecordStart()
    {
        CheckInferencing.text = "키를 눌러 녹음을 중지하세요. 녹음 중...";
        UpdateParams();
    }

    private void MicrophoneOnRecordStop(AudioClip clip)
    {
        CheckInferencing.text = "키를 눌러 녹음을 시작하세요.";
        FinalEvaluate(clip);
    }

    private async void Evaluate(AudioClip clip)
    {
        if (_task != null && !_task.IsCompleted)
            return;

        _task = _whisper.GetTextAsync(Microphone.GetAllClipData(), clip.frequency,
                clip.channels, _params);

        var res = await _task;

        string cleanTranscription = CleanText(res.Result);

        if (cleanTranscription == "" || cleanTranscription == null || string.IsNullOrWhiteSpace(cleanTranscription))
            return;

        Debug.Log(cleanTranscription);

        C_Talk talkPacket = new C_Talk();
        talkPacket.Message = cleanTranscription;
        Managers.Network.Send(talkPacket);

        Managers.STT.UpdateMySpeech(cleanTranscription);
    }

    private async void FinalEvaluate(AudioClip clip)
    {
        if (_task != null && !_task.IsCompleted)
            await _task;

        _task = _whisper.GetTextAsync(Microphone.GetAllClipData(), clip.frequency,
                clip.channels, _params);

        var res = await _task;

        string cleanTranscription = CleanText(res.Result);
        string transcription = cleanTranscription;

        if (string.IsNullOrWhiteSpace(cleanTranscription))
        {
            Microphone.ClearClips();
            return;
        }

        float score = 0;

        if (CurCommand.keywords != null)
        {
            score = Managers.Scenario.CheckKeywords(ref transcription);
            transcription += $"\n\n정확도 : {(score * 100).ToString("F2")}%\n\n";

            Debug.Log($"정확도 {score}");

            if (score > SimilarityThreshold)
            {
                CurCommand.thenDo.DynamicInvoke();
            }
        }

        Debug.Log(cleanTranscription);

        C_Talk talkPacket = new C_Talk();
        talkPacket.Message = cleanTranscription;
        Managers.Network.Send(talkPacket);

        Managers.STT.UpdateMySpeech(transcription);

        Microphone.ClearClips();
    }

    string CleanText(string input)
    {
        input = RemoveEncapsulatedText('[', ']', input);
        input = RemoveEncapsulatedText('(', ')', input);
        input = RemoveEncapsulatedText('*', '*', input);
        input = RemoveMultipleSpaces(input);
        input = ReplaceSpecificString(@"MBC", @"입니다.", input);
        input = ReplaceSpecificString(@"'구독'", @"주세요", input);
        input = ReplaceSpecificString(@"'구독'", @"드립니다", input);
        input = ReplaceMultipleDotsWithSingleDot(input);
        input = EnterAfterPoint(input);
        return input;
    }

    string ReplaceSpecificString(string start, string end, string input)
    {
        string pattern = start + ".*" + end;
        Regex regex = new Regex(pattern);
        return regex.Replace(input, "");
    }

    string EnterAfterPoint(string input)
    {
        return input.Replace(".", ".\n");
    }

    string RemoveEncapsulatedText(char start, char end, string input)
    {
        // Define the regular expression pattern to match text within square brackets
        string pattern = @"\" + start + @"[^\" + end + @"]*\" + end;

        // Create a regular expression object
        Regex regex = new Regex(pattern);

        // Use the regular expression to replace matches with an empty string
        string result = regex.Replace(input, "");

        return result;
    }

    string RemoveMultipleSpaces(string input)
    {
        // Define the regular expression pattern to match multiple spaces
        string pattern = @"\s+";

        // Create a regular expression object
        Regex regex = new Regex(pattern);

        // Use the regular expression to replace matches with a single space
        string result = regex.Replace(input, " ");

        return result;
    }

    string ReplaceMultipleDotsWithSingleDot(string input)
    {
        return Regex.Replace(input, @"\.{2,}", ".");
    }

    int LongestCommonSubsequence(string str1, string str2)
    {
        int m = str1.Length;
        int n = str2.Length;

        // Create a 2D array to store LCS lengths for subproblems
        int[,] dp = new int[m + 1, n + 1];

        // Build the LCS array in a bottom-up manner
        for (int i = 0; i <= m; i++)
        {
            for (int j = 0; j <= n; j++)
            {
                if (i == 0 || j == 0)
                {
                    dp[i, j] = 0; // Base case: LCS of an empty string and any other string is 0
                }
                else if (str1[i - 1] == str2[j - 1])
                {
                    dp[i, j] = dp[i - 1, j - 1] + 1; // Characters match, extend LCS by 1
                }
                else
                {
                    dp[i, j] = Math.Max(dp[i - 1, j], dp[i, j - 1]); // Characters don't match, get the max LCS from the previous cells
                }
            }
        }

        return dp[m, n]; // The bottom-right cell contains the length of the LCS
    }
}