using GoogleCloudStreamingSpeechToText;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class STTManager
{
    private AccumulateText _sttstreamingtext;
    public AccumulateText STTStreamingText
    {
        get
        {
            if (_sttstreamingtext == null)
            {
                GameObject go = GameObject.Find("GoogleCloudSpeechListener");
                if (go == null)
                    go = Managers.Resource.Instantiate("Prefabs/STT/GoogleCloudSpeechListener.prefab");
                StreamingRecognizer streamingRecognizer = go.GetComponent<StreamingRecognizer>();
                _sttstreamingtext = streamingRecognizer.TextUI.GetComponent<AccumulateText>();
            }

            return _sttstreamingtext;
        }

    }
    private StreamingRecognizer _googlespeechobj;
    public StreamingRecognizer GoogleSpeechObj
    {
        get
        {
            if (_googlespeechobj == null)
            {
                GameObject go = GameObject.Find("GoogleCloudSpeechListener");
                _googlespeechobj = go.GetComponent<StreamingRecognizer>();
            }
            return _googlespeechobj;
        }
    }
    private string _speechBuffer;
    public string SpeechBuffer
    {
        get
        {
            return _speechBuffer;
        }
        set
        {
            _speechBuffer = value;
        }
    }

    GameObject _mySpeech;
    public GameObject MySpeech
    {
        get
        {
            if (_mySpeech == null)
                _mySpeech = GameObject.Find("MySpeech");

            if (_mySpeech == null)
                _mySpeech = Managers.UI.CreateUI("MySpeech");

            if (_mySpeech == null)
                Debug.LogError("Can't find prefab : MySpeech");

            return _mySpeech;
        }
    }

    GameObject _openSpeech;
    public GameObject OpenSpeech
    {
        get
        {
            if (_openSpeech == null)
                _openSpeech = MySpeech.transform.GetChild(0).gameObject;

            return _openSpeech;
        }
    }

    GameObject _closeSpeech;
    public GameObject CloseSpeech
    {
        get
        {
            if (_closeSpeech == null)
                _closeSpeech = MySpeech.transform.GetChild(1).gameObject;

            return _closeSpeech;
        }
    }

    bool _isClosed = true;

    Coroutine _textAnim;

    public void UpdateMySpeech(string message, bool reset = true)
    {
        if (message == null)
            return;

        if (reset)
            ResetBuffer();

        _speechBuffer = message;

        ChangeSpeechState(false);

        //if (_textAnim != null)
        //    Managers.Instance.StopCoroutine(_textAnim);
        //수정
        //_textAnim = Managers.Instance.StartCoroutine(OpenSpeech.GetComponent<TextTwinkle>().Typing(_speechBuffer, OpenSpeech.transform.GetChild(0).gameObject));
    }

    public void ChangeSpeechState()
    {
        ChangeSpeechState(!_isClosed);
    }

    /// <summary>
    /// 말풍선이 열릴 때 false, 닫힐 때 true
    /// </summary>
    /// <param name="isClosed"></param>
    void ChangeSpeechState(bool isClosed)
    {
        _isClosed = isClosed;
        OpenSpeech.SetActive(!_isClosed);
        CloseSpeech.SetActive(_isClosed);
    }

    public void CloseMySpeech()
    {
        ChangeSpeechState(true);
    }

    public void OpenMySpeech()
    {
        ChangeSpeechState(false);
    }

    public void ResetBuffer()
    {
        SpeechBuffer = null;
    }
}
