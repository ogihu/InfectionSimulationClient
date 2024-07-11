using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Whisper;
using static Define;

public class ScenarioManager
{
    public int CompleteCount { get; set; }

    public string ScenarioName { get; set; }
    public int Progress { get; set; }
    public string Equipment { get; set; }

    Dictionary<string, GameObject> NPCs = new Dictionary<string, GameObject>();

    GameObject _speechRecognitor;
    public GameObject SpeechRecognitor
    {
        get
        {
            if (_speechRecognitor == null)
                _speechRecognitor = GameObject.Find("SpeechRecognitor");

            if (_speechRecognitor == null)
                _speechRecognitor = Managers.Resource.Instantiate("System/SpeechRecognitor");

            return _speechRecognitor;
        }
    }

    GameObject _scenarioAssist;
    public GameObject ScenarioAssist
    {
        get
        {
            if (_scenarioAssist == null)
                _scenarioAssist = GameObject.Find("ScenarioAssist");

            if (_scenarioAssist == null)
                _scenarioAssist = Managers.UI.CreateUI("ScenarioAssist");

            return _scenarioAssist;
        }
    }

    #region 시나리오 수행 결과 저장 버퍼

    public string MyAction { get; set; }
    public string SpeechText { get; set; }  //STT 결과 저장
    public List<string> Targets { get; set; } = new List<string>();

    #endregion

    /// <summary>
    /// 이미 Complete 패킷을 보냈는지 확인, 서버에서 시나리오는 진행시키면 (NextProcess 패킷을 받으면) false로 전환해야 함.
    /// </summary>
    private bool _checkComplete;
    public Coroutine _routine;

    ScenarioInfo CurrentScenarioInfo { get; set; }

    public void Init(string scenarioName)
    {
        ScenarioName = scenarioName;
        Progress = 0;
        CompleteCount = 0;
        _checkComplete = false;
        SpeechText = null;
        CurrentScenarioInfo = Managers.Data.ScenarioData[ScenarioName][Progress];
    }

    void Reset()
    {
        if (SpeechText != null || MyAction != null || Targets.Count > 0)
        {
            SpeechText = null;
            MyAction = null;
            Targets.Clear();
        }
    }

    #region 시나리오 패킷 관련 기능

    public void SendScenarioInfo(string scenarioName)
    {
        C_StartScenario scenarioPacket = new C_StartScenario();
        scenarioPacket.ScenarioName = scenarioName;
        Managers.Network.Send(scenarioPacket);
    }

    void SendComplete()
    {
        if (_checkComplete == false)
        {
            C_Complete packet = new C_Complete();
            Managers.Network.Send(packet);
            _checkComplete = true;
        }
    }

    #endregion

    #region 시나리오 진행 기능

    public void StartScenario(string scenarioName)
    {
        Managers.Instance.StartCoroutine(CoScenario(scenarioName));
    }

    IEnumerator CoScenarioStep(int progress)
    {
        if (Managers.Object.MyPlayer.Position == CurrentScenarioInfo.Position)
        {
            UpdateScenarioAssist("시나리오를 진행하세요.");
            Managers.Instance.StartCoroutine(CoCheckAction());
            yield return new WaitUntil(() => CompleteCount >= 1);
        }
        else
        {
            UpdateScenarioAssist("다른 플레이어가 시나리오를 진행 중 입니다...");
        }
        SendComplete();

        yield return new WaitUntil(() => Progress == progress);
    }

    IEnumerator CoScenario(string scenarioName)
    {
        Managers.UI.CreatePopup($"{scenarioName} 시나리오를 시작합니다.");
        yield return new WaitForSeconds(3.0f);

        Managers.UI.CreatePopup($"사랑합니다.\n지금부터 신종감염병 대응 모의 훈련을 시작하고자 하오니 환자 및 보호자께서는 동요하지 마시기 바랍니다.\n모의 훈련 요원들은 지금부터 훈련을 시작하도록 하겠습니다.");
        yield return new WaitForSeconds(3.0f);

        Init(scenarioName);
        NPCs["환자"].transform.position = new Vector3(11.5f, 0, 0);
        NPCs["환자"].transform.rotation = Quaternion.Euler(0, -90, 0);

        switch (scenarioName)
        {
            case "엠폭스":
                Managers.UI.CreateChatUI(NPCs["환자"].transform, "선생님 방금 가족 중에 한명이 보건소로부터 엠폭스 확진받았다고 연락을 받아서요.\n저도 곧 보건소로부터 연락올거라고 합니다.");
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(1));
                Managers.UI.CreateChatUI(NPCs["환자"].transform, "이관리 980421 입니다.\n같이 살고있어요.");
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(2));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(3));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(4));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(5));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(6));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(7));

                break;
        }

        Managers.UI.CreatePopup($"{scenarioName} 시나리오를 완료하셨습니다.");
    }

    #endregion

    #region 시나리오 진행 관련 기능

    //현재 진행된 시나리오에 대하여 다른 플레이어들이 확인할 수 있도록 말풍선, 메시지 등의 상황을 업데이트 해주는 함수
    void UpdateSituation()
    {
        switch (CurrentScenarioInfo.Action)
        {
            case "Call":
                Managers.Phone.Device.SendMessage(CurrentScenarioInfo.Position, CurrentScenarioInfo.Speech, CurrentScenarioInfo.Targets);
                break;
            case "Tell":
                BaseController player = Managers.Object.FindPosition(CurrentScenarioInfo.Position).GetComponent<BaseController>();

                if (player == null)
                    break;

                Managers.UI.CreateChatUI(player.transform, CurrentScenarioInfo.Speech);
                break;
        }
    }

    //서버로부터 다음 시나리오 진행도를 받으면, 시나리오 상황 업데이트 및 변수 초기화 등 실행
    public void NextProgress(int progress)
    {
        Managers.UI.ClearChat();

        UpdateSituation();

        Progress = progress;
        CompleteCount = 0;
        _checkComplete = false;
        Reset();
        CurrentScenarioInfo = Managers.Data.ScenarioData[ScenarioName][Progress];
        _routine = null;
    }

    //화면 상단에 시나리오 진행 관련 힌트를 주는 UI 업데이트
    public void UpdateScenarioAssist(string state, string position = null)
    {
        if (position != null)
            ScenarioAssist.transform.GetChild(0).GetComponent<TMP_Text>().text = position;

        ScenarioAssist.transform.GetChild(1).GetComponent<TMP_Text>().text = state;
    }

    public GameObject AddNPC(string position, Vector3 spawnPoint)
    {
        return null;
    }

    #endregion

    #region 시나리오 검증 관련 기능

    IEnumerator CoCheckAction()
    {
        ChangeKeyword(CurrentScenarioInfo.Keywords);
        bool complete = false;

        while (!complete)
        {
            yield return new WaitUntil(() => CheckCondition());
            complete = CheckAction();
            Reset();
        }

        CompleteCount++;
    }

    bool CheckCondition()
    {
        if (MyAction == null)
            return false;

        if (CurrentScenarioInfo.Keywords.Count > 0)
            if (SpeechText == null)
                return false;

        return true;
    }

    void ChangeKeyword(List<string> keywords)
    {
        SpeechRecognitor.GetComponent<WhisperManager>().initialPrompt = "";
        foreach (var keyword in keywords)
        {
            SpeechRecognitor.GetComponent<WhisperManager>().initialPrompt += $"{keyword} ";
        }
    }

    bool CheckAction()
    {
        if (MyAction != CurrentScenarioInfo.Action)
        {
            Managers.UI.CreatePopup("올바른 행동을 수행하지 않았습니다.");
            return false;
        }

        if (!CheckTarget())
        {
            Managers.UI.CreatePopup("대상이 올바르지 않습니다.");
            return false;
        }

        if(Equipment != CurrentScenarioInfo.Equipment)
        {
            Managers.UI.CreatePopup("올바른 장비를 착용하지 않았습니다.");
            return false;
        }

        if (!CheckSpeech())
        {
            return false;
        }

        return true;
    }

    bool CheckSpeech()
    {
        if (SpeechText == null)
        {
            Managers.UI.CreatePopup("녹음이 정상적으로 수행되지 않았습니다. 다시 시도 해주세요");
            return false;
        }

        int count = 0;

        foreach (var keyword in CurrentScenarioInfo.Keywords)
        {
            if (SpeechText.Contains(keyword))
                count++;
        }

        float ratio = (float)count / (float)CurrentScenarioInfo.Keywords.Count;

        GameObject go = Managers.UI.CreateUI("MySpeech");
        go.GetComponentInChildren<TMP_Text>().text = $"{SpeechText}\n\n정확도 {ratio * 100}%";
        Managers.Instance.StartCoroutine(Managers.UI.DestroyAfter(go, 3.0f));
        SpeechText = null;

        if (ratio > 0.7f)
            return true;
        else
        {
            go.GetComponentInChildren<TMP_Text>().text += "\n정확도가 낮습니다. 시나리오를 다시 시도해주세요.";
            return false;
        }
    }

    bool CheckTarget()
    {
        if (Targets.Count != CurrentScenarioInfo.Targets.Count)
            return false;

        foreach(var target in CurrentScenarioInfo.Targets)
        {
            if (!Targets.Contains(target))
                return false;
        }

        return true;
    }

    #endregion
}
