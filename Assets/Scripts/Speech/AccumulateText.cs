using Google.Protobuf.Collections;
using Google.Protobuf.Protocol;
using GoogleCloudStreamingSpeechToText;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(TextMeshProUGUI))]
public class AccumulateText : MonoBehaviour
{
    public string _accumulatedText = "";
    public string _interimText = "";
    public  TextMeshProUGUI _text;
    private RectTransform _canvasRect;
    public float score = 0;
    private class Command
    {
        public List<string> keywords;
        public Action thenDo;
    }
    private Command CurCommand { get; set; }

    [Range(0f, 1f)]
    [Tooltip("How similar must the user's transcription be to trigger a match?")]
    public float SimilarityThreshold;


    public void AddInterimTranscription(string interimTranscription)
    {
        _interimText = _accumulatedText == "" ? interimTranscription : " " + interimTranscription;
        SetText();
    }

    public void AddFinalTranscription(string finalTranscription)
    {
        if (_accumulatedText != "")
        {
            _accumulatedText += " ";
        }
        _accumulatedText += finalTranscription;
        _interimText = "";
        SetText();
        //FinalEvaluate();
    }


    private void Awake()
    {
        _text = gameObject.GetComponent<TextMeshProUGUI>();
        _canvasRect = _text.canvas.GetComponent<RectTransform>();
    }
    public bool Stoptext = false;
    private void SetText()
    {
        if (Stoptext)
            return;
        float textHeight = LayoutUtility.GetPreferredHeight(_text.rectTransform);
        float parentHeight = _canvasRect.rect.height;
        if (textHeight > parentHeight)
        {
            _accumulatedText = "";
            _text.text = _interimText.Trim();
        }
        else
        {
            _text.text = _accumulatedText + _interimText;
        }
        
    }
    public  void FinalEvaluate()
    {
        if (Managers.Scenario._doingScenario == false)
            return;

        string transcription = _text.text;
        string content = transcription;

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

        Debug.Log(transcription);
        _text.text = transcription;

        if (!string.IsNullOrEmpty(content))
        {
            C_Talk talkPacket = new C_Talk();
            talkPacket.Message = content;
            Managers.Network.Send(talkPacket);
        }
    }

    public void ThenDo()
    {
        if (String.IsNullOrEmpty(Managers.Scenario.CurrentScenarioInfo.Place))
            Managers.Scenario.PassSpeech = true;
        else
        {
            if (Managers.Scenario.CurrentScenarioInfo.Place == Managers.Object.MyPlayer.Place)
                Managers.Scenario.PassSpeech = true;
        }
    }

    public  void RegisterCommand(string command, bool player)
    {
        initializeCommands();

        if (player)
        {
            CurCommand.keywords = Managers.Scenario.CurrentScenarioInfo.Keywords;
            CurCommand.thenDo = ThenDo;
            StreamingRecognizer.needKeyword.Clear();
            StreamingRecognizer.needKeyword.AddRange(CurCommand.keywords);
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
}
