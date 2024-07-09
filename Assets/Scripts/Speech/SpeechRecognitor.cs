using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;
using Whisper;
using Whisper.Utils;
using Whisper.Utils.Extension;
using Button = UnityEngine.UI.Button;
using Toggle = UnityEngine.UI.Toggle;
using TMPro;

public class SpeechRecognitor : MonoBehaviour
{
    WhisperManager whisper;
    public MicRecord microphoneRecord;

    public bool streamSegments = true;
    public bool printLanguage = true;

    private string _buffer;

    private void Awake()
    {
        whisper = GetComponent<WhisperManager>();
        microphoneRecord = GetComponent<MicRecord>();

        whisper.OnNewSegment += OnNewSegment;

        microphoneRecord.OnRecordStop += OnRecordStop;
    }

    private void Update()
    {
        OnRecordButtonPressed();
    }

    void OnRecordButtonPressed()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            Managers.Scenario.MyAction = "Tell";
            microphoneRecord.StartRecord();
        }
        else if(Input.GetKeyUp(KeyCode.T))
            microphoneRecord.StopRecord();
    }

    private async void OnRecordStop(Whisper.Utils.Extension.AudioChunk recordedAudio)
    {
        _buffer = "";

        var sw = new Stopwatch();
        sw.Start();

        var res = await whisper.GetTextAsync(recordedAudio.Data, recordedAudio.Frequency, recordedAudio.Channels);
        if (res == null)
            return;

        //var time = sw.ElapsedMilliseconds;
        //var rate = recordedAudio.Length / (time * 0.001f);
        //string duringTime = $"Time: {time} ms\nRate: {rate:F1}x";

        var text = res.Result;
        //if (printLanguage)
        //    text += $"\nLanguage: {res.Language}\n{duringTime}";
        Managers.UI.CreatePopup(text);
        Managers.Scenario.SpeechText = text;
    }

    private void OnNewSegment(WhisperSegment segment)
    {
        if (!streamSegments)
            return;

        _buffer += segment.Text;
    }
}