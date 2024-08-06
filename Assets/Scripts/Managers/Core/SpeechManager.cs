using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SpeechManager
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

    public void UpdateMySpeech(string message)
    {
        if (message == null)
            return;

        _speechBuffer = message;

        MySpeech.SetActive(true);
        MySpeech.GetComponent<TextTwinkle>().PrintText(_speechBuffer, MySpeech);
    }

    public void CloseMySpeech(float time)
    {
        Managers.Instance.StartCoroutine(Managers.UI.InvisibleAfter(MySpeech, time));
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
