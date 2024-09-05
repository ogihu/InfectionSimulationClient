using GoogleCloudStreamingSpeechToText;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AIStateCheck : MonoBehaviour
{
    bool _isInferencing;
    GameObject _text;
    Coroutine _co;

    private void Awake()
    {
        Managers.STT.OpenSpeech.SetActive(true);
        _text = Managers.STT.OpenSpeech.transform.GetChild(6).gameObject;
        Managers.STT.OpenSpeech.SetActive(false);
        _isInferencing = false;
        ChangeState(_isInferencing);
    }

    void Update()
    {
        if (_isInferencing != Managers.STT.GoogleSpeechObj.TalkCheck)
        {
            _isInferencing = Managers.STT.GoogleSpeechObj.TalkCheck;
            ChangeState(_isInferencing);
        }
    }

    public void ChangeState(bool tasking)
    {
        if (!tasking)
        {
            if (_co != null)
                StopCoroutine(_co);

            _text.GetComponent<TMP_Text>().text = "";
        }
        else
        {
            _co = StartCoroutine(_text.GetComponent<TextTwinkle>().SelectedWordsLoop("추론 중", "...", _text));
        }
    }
}
