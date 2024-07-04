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
    public string _selectedFunc;
    public List<string> _choosedAddress = new List<string>();

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
        _selectedFunc = funcName;
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
        _targetToggle.ForEach((x) => { if (x.isOn == true) { _choosedAddress.Add(x.gameObject.name); } });

        switch (_selectedFunc)
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

    /// <summary>
    /// 올바른 기능을 사용했으면 true, 아니면 false 리턴
    /// </summary>
    /// <param name="function"></param>
    /// <returns></returns>
    public bool CheckFunction(string function)
    {
        if (_selectedFunc == null)
            return false;

        if (_selectedFunc == function)
            return true;

        return false;
    }

    /// <summary>
    /// 연락처를 올바르게 선택했으면 true, 아니면 false 리턴
    /// </summary>
    /// <param name="targetArray"></param>
    /// <returns></returns>
    public bool CheckTargets(string[] targetArray)
    {
        if(_choosedAddress.Count != targetArray.Length)
            return false;

        foreach (string target in targetArray)
        {
            if (!_choosedAddress.Contains(target))
                return false;
        }

        return true;
    }
}
