using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SmartPhone : MonoBehaviour
{
    #region Interfaces

    GameObject _speechRecognitor;

    GameObject _functions;

    GameObject _targets;
    List<Toggle> _targetToggle = new List<Toggle>();

    GameObject _calling;

    #endregion

    bool _initialized = false;

    void OnEnable()
    {
        if (!_initialized)
        {
            Init();
        }
        Reset();
        Managers.Object.MyPlayer._playerState = Define.PlayerState.UsingPhone;
    }

    private void OnDisable()
    {
        Reset();
        Managers.Object.MyPlayer._playerState = Define.PlayerState.None;
    }

    public void Init()
    {
        _speechRecognitor = GameObject.Find("SpeechRecognitor");
        _functions = Util.FindChildByName(gameObject, "Functions");
        _targets = Util.FindChildByName(gameObject, "Targets");
        _calling = Util.FindChildByName(gameObject, "Calling");

        GameObject content = Util.FindChildByName(_targets, "Content");
        for(int i = 0; i < Define.PhoneAddress.Length; i++)
        {
            GameObject go = Managers.Resource.Instantiate("UI/PhoneAddress", content.transform);
            go.transform.GetChild(1).GetComponent<Text>().text = Define.PhoneAddress[i];
            go.name = Define.PhoneAddress[i];
            _targetToggle.Add(go.GetOrAddComponent<Toggle>());
        }

        _initialized = true;
    }

    public void Reset()
    {
        _functions.SetActive(true);
        _targets.SetActive(false);
        _calling.SetActive(false);
        _targetToggle.ForEach((x) => { x.isOn = false; });
    }

    public void FuncSelect(string funcName)
    {
        Managers.Scenario.MyAction = funcName;
        _functions.SetActive(false);
        _targets.SetActive(true);
        _calling.SetActive(false);
    }

    public void FuncCancel()
    {
        Reset();
    }

    public void FuncConfirm()
    {
        Managers.Scenario.Targets.Clear();
        _targetToggle.ForEach((x) => { if (x.isOn == true) { Managers.Scenario.Targets.Add(x.gameObject.name); } });

        switch (Managers.Scenario.MyAction)
        {
            case "Call":
                _speechRecognitor.GetComponent<SpeechRecognitor>().microphoneRecord.StartRecord();
                _functions.SetActive(false);
                _targets.SetActive(false);
                _calling.SetActive(true);
                break;
            case "KakaoTalk":
                Managers.Phone.ClosePhone();
                break;
        }
    }

    public void FinishCall()
    {
        _speechRecognitor.GetComponent<SpeechRecognitor>().microphoneRecord.StopRecord();
        Managers.Phone.ClosePhone();
    }
}
