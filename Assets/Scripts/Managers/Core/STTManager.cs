using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class STTManager
{
    private RealtimeSTTManager _sttManager;
    public RealtimeSTTManager SttManager
    {
        get
        {
            if (_sttManager == null)
            {
                GameObject go = GameObject.Find("RealtimeSTT");

                if (go == null)
                    go = Managers.Resource.Instantiate("System/RealtimeSTT");

                if (go == null)
                    Debug.LogError("Can't find prefab : RealtimeSTT");

                _sttManager = go.GetComponent<RealtimeSTTManager>();
            }

            return _sttManager;
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

            if( _mySpeech == null)
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
            if(_openSpeech == null)
                _openSpeech = MySpeech.transform.GetChild(0).gameObject;

            return _openSpeech;
        }
    }

    GameObject _closeSpeech;
    public GameObject CloseSpeech
    {
        get
        {
            if(_closeSpeech == null)
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

        if (_textAnim != null)
            Managers.Instance.StopCoroutine(_textAnim);

        _textAnim = Managers.Instance.StartCoroutine(OpenSpeech.GetComponent<TextTwinkle>().Typing(_speechBuffer, OpenSpeech.transform.GetChild(0).gameObject));
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

    public void StartSpeech()
    {
        SttManager.Microphone.StartRecord();
    }

    public void StopSpeech()
    {
        SttManager.Microphone.StopRecord();
    }

    public void ResetBuffer()
    {
        SpeechBuffer = null;
    }
}
