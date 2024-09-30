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
        Managers.Object.MyPlayer.State = Google.Protobuf.Protocol.CreatureState.UsingPhone;
    }

    private void OnDisable()
    {
        Reset();
        Managers.Object.MyPlayer.State = Google.Protobuf.Protocol.CreatureState.Idle;
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
            case "Messenger":
                if (Managers.Scenario.CurrentScenarioInfo == null)
                {
                    Managers.UI.CreateSystemPopup("WarningPopup", "해당 기능은 필요한 상황에만 사용할 수 있습니다.");
                    return;
                }

                if (!(Managers.Scenario.CurrentScenarioInfo.Action == "Call" || Managers.Scenario.CurrentScenarioInfo.Action == "Messenger"))
                {
                    Managers.UI.CreateSystemPopup("WarningPopup", "해당 기능은 필요한 상황에만 사용할 수 있습니다.");
                    return;
                }

                Managers.Scenario.MyAction = funcName;
                _functions.SetActive(false);
                _targets.SetActive(true);
                _messages.SetActive(false);
                _calling.SetActive(false);
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

        switch (Managers.Scenario.MyAction)
        {
            case "Call":
                Managers.STT.GoogleSpeechObj.GetComponent<StreamingRecognizer>().TextUI.GetComponent<AccumulateText>()._text.text = "";
                Managers.STT.GoogleSpeechObj.GetComponent<StreamingRecognizer>().TextUI.GetComponent<AccumulateText>()._accumulatedText = "";
                Managers.STT.GoogleSpeechObj.GetComponent<StreamingRecognizer>().TextUI.GetComponent<AccumulateText>()._interimText = "";
                Managers.STT.GoogleSpeechObj.StartListening();
                _functions.SetActive(false);
                _targets.SetActive(false);
                _calling.SetActive(true);
                _isCalling = true;
                break;
            case "Messenger":
                Managers.Phone.ClosePhone();
                break;
        }
    }

    public void UpdateMessageList()
    {
        if (Messages.Count <= 0)
            return;

        GameObject content = Util.FindChildByName(_messages, "Content");
        for(int i = Messages.Count - 1; i >= 0; i--)
        {
            GameObject go = Managers.UI.CreateUI("Message", content.transform);
            go.GetComponent<ButtonMessage>().Init(Messages[i]);
        }
    }

    public void FinishCall()
    {
        _isCalling = false;
        Managers.STT.GoogleSpeechObj.StopListening();
        Managers.STT.GoogleSpeechObj.GetComponent<StreamingRecognizer>().TextUI.GetComponent<AccumulateText>().FinalEvaluate();
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
