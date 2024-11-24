using UnityEngine;
using System.IO;
using System.Net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Networking;

[System.Serializable]
public class ApiKeyConfig
{
    public string tts_api_key; // JSON의 "tts_api_key"에 매핑
}

public class TTSManager
{
    string apiURL;
    private const string apiKeyFileName = "tts_api_key.json";
    SetTextToSpeech tts = new SetTextToSpeech();
    List<AudioSource> ttsAudios = new List<AudioSource>();

    public void Init()
    {
        LoadApiKey();
    }

    public void Speaking(Transform host, string message)
    {
        AudioSource audioSource = host.GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource = host.gameObject.AddComponent<AudioSource>();
        }

        bool isPlayerNPC = false;
        if (host.GetComponent<PlayerNPCController>() != null)
        {
            Managers.Scenario.TTSPlaying = true;
            ttsAudios.Add(audioSource);
            isPlayerNPC = true;
        }
            
        SetVoice();
        tts.input.text = message;

        // 비동기적으로 음성 생성
        Managers.Instance.StartCoroutine(CreateAudioCoroutine(audioSource, isPlayerNPC));
    }

    private void LoadApiKey()
    {
        // `StreamingAssets` 경로 가져오기
        string filePath = Path.Combine(Application.streamingAssetsPath, apiKeyFileName);

        if (!File.Exists(filePath))
        {
            Debug.LogError($"API key file not found at {filePath}");
            return;
        }

        // 파일에서 JSON 읽기
        string jsonContent = File.ReadAllText(filePath);
        ApiKeyConfig config = JsonUtility.FromJson<ApiKeyConfig>(jsonContent);

        if (string.IsNullOrEmpty(config.tts_api_key))
        {
            Debug.LogError("API key not found in the file!");
            return;
        }

        apiURL = config.tts_api_key; // API URL 설정
        Debug.Log("API URL loaded successfully.");
    }

    void SetVoice()
    {
        SetInput si = new SetInput();
        si.text = "";
        tts.input = si;

        SetVoice sv = new SetVoice();
        sv.languageCode = "ko-KR";
        sv.name = "ko-KR-Wavenet-C";
        sv.ssmlGender = "MALE";
        tts.voice = sv;

        SetAudioConfig sa = new SetAudioConfig();
        sa.audioEncoding = "LINEAR16";
        sa.speakingRate = 1f;
        sa.pitch = 0;
        sa.volumeGainDb = 0;
        tts.audioConfig = sa;
    }

    private IEnumerator CreateAudioCoroutine(AudioSource audioSource, bool isPlayerNPC)
    {
        yield return Managers.Instance.StartCoroutine(TextToSpeechPostCoroutine(tts, (responseStr) =>
        {
            if (string.IsNullOrEmpty(responseStr))
            {
                Debug.LogError("Failed to get TTS response.");
                return;
            }

            GetContent info = JsonUtility.FromJson<GetContent>(responseStr);

            if (info == null)
            {
                Debug.LogError("Failed to parse TTS response.");
                return;
            }

            var bytes = Convert.FromBase64String(info.audioContent);
            var f = ConvertByteToFloat(bytes);

            AudioClip audioClip = AudioClip.Create("audioContent", f.Length, 1, 24000, false);
            audioClip.SetData(f, 0);

            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }

            Managers.Sound.Play(audioSource, audioClip);

            if (isPlayerNPC)
                Managers.Instance.StartCoroutine(WaitForAudioToEnd(audioSource));
        }));
    }

    private IEnumerator WaitForAudioToEnd(AudioSource audioSource)
    {
        yield return new WaitUntil(() => !audioSource.isPlaying);

        ttsAudios.Remove(audioSource);

        if (ttsAudios.Count <= 0)
            Managers.Scenario.TTSPlaying = false;
    }

    public static float[] ConvertByteToFloat(byte[] array)
    {
        float[] floatArr = new float[array.Length / 2];

        for (int i = 0; i < floatArr.Length; i++)
        {
            floatArr[i] = BitConverter.ToInt16(array, i * 2) / 32768.0f;
        }
        return floatArr;
    }

    private IEnumerator TextToSpeechPostCoroutine(object data, Action<string> callback)
    {
        string json = JsonUtility.ToJson(data);
        byte[] bytes = Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest request = new UnityWebRequest(apiURL, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bytes);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError(request.error);
                callback?.Invoke(null);
            }
            else
            {
                callback?.Invoke(request.downloadHandler.text);
            }
        }
    }
}


public class SetTextToSpeech
{
    //input 필드에서 text와 ssml을 사용할 수 있고 둘 중 하나만 사용가능 반드시 하나는 사용해야함
    public SetInput input;
    //목소리에 관한부분
    public SetVoice voice;
    //오디오 형식에 관한 부분
    public SetAudioConfig audioConfig;
}
[System.Serializable]
public class SetInput
{
    public string text;

    //public string ssml; SSML은 음성 합성 마크업 언어(사용하지 않으면 주석)
}
[System.Serializable]
public class SetVoice
{
    //언어 설정, 영어 한국어 등
    public string languageCode;
    //목소리 이름(버전)
    public string name;
    //성별
    public string ssmlGender;
}
[System.Serializable]
public class SetAudioConfig
{
    //오디오 인코딩 방식
    public string audioEncoding;
    //말하는 속도
    public float speakingRate;
    //높낮이
    public int pitch;
    //크기
    public int volumeGainDb;
}
[System.Serializable]
public class GetContent
{
    public string audioContent;
}

