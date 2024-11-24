using Google.Protobuf.Protocol;
using GoogleCloudStreamingSpeechToText;
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

    GameObject _messages;

    #endregion

    public List<Message> Messages { get; set; } = new List<Message>();
    bool _initialized = false;
    public bool _isCalling = false;

    private void OnEnable()
    {
        if (!_initialized)
        {
            Init();
        }
        Reset();
    }

    private void OnDisable()
    {
        Reset();
    }

    public void Init()
    {
        _speechRecognitor = GameObject.Find("SpeechRecognitor");
        _functions = Util.FindChildByName(gameObject, "Functions");
        _targets = Util.FindChildByName(gameObject, "Targets");
        _calling = Util.FindChildByName(gameObject, "Calling");
        _messages = Util.FindChildByName(gameObject, "Messages");

        GameObject content = Util.FindChildByName(_targets, "Content");
        for (int i = 0; i < Define.PhoneAddress.Length; i++)
        {
            GameObject go = Managers.UI.CreateUI("PhoneAddress", content.transform);
            go.GetComponent<ToggleUI>().SetLabel(Define.PhoneAddress[i]);
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
        _messages.SetActive(false);
        _targetToggle.ForEach((x) => { x.isOn = false; });
    }

    public void FuncSelect(string funcName)
    {
        switch (funcName)
        {
            case "Call":
                if (Managers.Scenario.CurrentScenarioInfo == null)
                {
                    Managers.UI.CreateSystemPopup("WarningPopup", "시나리오가 시작되지 않았습니다.", UIManager.NoticeType.None);
                    return;
                }

                if (!(Managers.Scenario.CurrentScenarioInfo.Action == "Call"))
                {
                    Managers.UI.CreateSystemPopup("WarningPopup", "해당 기능은 현재 사용할 수 없습니다.", UIManager.NoticeType.None);
                    return;
                }

                if (!Managers.Scenario.CheckPlace())
                    return;

                Managers.Scenario.MyAction = funcName;
                _functions.SetActive(false);
                _targets.SetActive(true);
                _messages.SetActive(false);
                _calling.SetActive(false);
                break;
            case "Messenger":
                if (Managers.Scenario.CurrentScenarioInfo == null)
                {
                    Managers.UI.CreateSystemPopup("WarningPopup", "시나리오가 시작되지 않았습니다.", UIManager.NoticeType.None);
                    return;
                }

                if (!(Managers.Scenario.CurrentScenarioInfo.Action == "Messenger"))
                {
                    Managers.UI.CreateSystemPopup("WarningPopup", "해당 기능은 현재 사용할 수 없습니다.", UIManager.NoticeType.None);
                    return;
                }

                if (!Managers.Scenario.CheckPlace())
                    return;

                Managers.Scenario.MyAction = funcName;
                Managers.UI.CreateSystemPopup("WarningPopup", "신속대응팀을 활성화 하였습니다.", UIManager.NoticeType.Info);
                Managers.Phone.ClosePhone();
                break;
            case "MessageCheck":
                UpdateMessageList();
                _functions.SetActive(false);
                _targets.SetActive(false);
                _messages.SetActive(true);
                _calling.SetActive(false);
                break;
        }

    }

    public void FuncCancel()
    {
        Reset();
    }

    public void FuncConfirm()
    {
        Managers.Scenario.Targets.Clear();

        _targetToggle.ForEach((x) => { if (x.isOn == true) { Managers.Scenario.Targets.Add(x.gameObject.name); } });

        if (!Managers.Scenario.CheckTarget())
        {
            Managers.UI.CreateSystemPopup("WarningPopup", "대상이 올바르지 않습니다.", UIManager.NoticeType.Warning);
            Reset();
            return;
        }

        switch (Managers.Scenario.MyAction)
        {
            case "Call":
                if (Managers.Setting.UsingMic)
                {
                    Managers.STT.GoogleSpeechObj.GetComponent<CustomStreamingRecognizer>().TextUI.GetComponent<AccumulateText>()._text.text = "";
                    Managers.STT.GoogleSpeechObj.GetComponent<CustomStreamingRecognizer>().TextUI.GetComponent<AccumulateText>()._accumulatedText = "";
                    Managers.STT.GoogleSpeechObj.GetComponent<CustomStreamingRecognizer>().TextUI.GetComponent<AccumulateText>()._interimText = "";
                    Managers.STT.GoogleSpeechObj.StartListening();
                    _functions.SetActive(false);
                    _targets.SetActive(false);
                    _calling.SetActive(true);
                    _isCalling = true;
                }
                else
                {
                    Managers.Phone.ClosePhone();
                    Managers.Keyword.OpenGUIKeyword();
                }
                break;
            case "Messenger":
                break;
        }
    }

    public void UpdateMessageList()
    {
        if (Messages.Count <= 0)
            return;

        GameObject content = Util.FindChildByName(_messages, "Content");
        for (int i = Messages.Count - 1; i >= 0; i--)
        {
            GameObject go = Managers.UI.CreateUI("Message", content.transform);
            go.GetComponent<ButtonMessage>().Init(Messages[i]);
        }
    }

    public void FinishCall()
    {
        _isCalling = false;
        Managers.STT.GoogleSpeechObj.StopListening();
        Managers.STT.GoogleSpeechObj.GetComponent<CustomStreamingRecognizer>().TextUI.GetComponent<AccumulateText>().FinalEvaluate();
        Managers.Phone.ClosePhone();
    }

    public void SendMessage(string sender, string content, List<string> receivers)
    {
        string[] split = Managers.Object.MyPlayer.Position.Split(' ');

        if (!receivers.Contains(split[0]))
            return;

        Message message = new Message();
        message.Sender = sender;
        message.Content = content;
        Messages.Add(message);
    }
}
