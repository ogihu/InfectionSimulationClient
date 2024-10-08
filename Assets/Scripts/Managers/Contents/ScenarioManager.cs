using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.AI;
using static Define;

public class ScenarioManager
{
    public int CompleteCount { get; set; }

    public string ScenarioName { get; set; }
    public int Progress { get; set; }
    public string Item { get; set; }

    public Dictionary<string, NPCController> NPCs = new Dictionary<string, NPCController>();

    GameObject _realtimeSTT;
    public GameObject RealtimeSTT
    {
        get
        {
            if (_realtimeSTT == null)
                _realtimeSTT = GameObject.Find("RealtimeSTT");

            if (_realtimeSTT == null)
                _realtimeSTT = Managers.Resource.Instantiate("System/RealtimeSTT");

            return _realtimeSTT;
        }
    }

    GameObject _scenarioAssist;
    bool _scenarioHint = false;
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

    public void ScenarioAssist_HintActive()
    {
        if (_scenarioHint)
        {
            _scenarioAssist.transform.GetChild(2).gameObject.SetActive(false);
            _scenarioHint = false;
        }
        else
        {
            _scenarioAssist.transform.GetChild(2).gameObject.SetActive(true);
            _scenarioHint = true;
        }
    }

    #region 시나리오 수행 결과 저장 버퍼

    public string MyAction { get; set; }
    public bool PassSpeech { get; set; }
    public List<string> Targets { get; set; } = new List<string>();

    #endregion

    private bool _checkComplete;    //이미 Complete 패킷을 보냈는지 확인, 서버에서 시나리오는 진행시키면 (NextProcess 패킷을 받으면) false로 전환해야 함.
    public Coroutine _routine;
    public bool _doingScenario = false;
    public int PopupConfirm { get; set; }   //평상시에는 0, CheckPopup에서 확인을 선택했으면 1, 취소를 선택했으면 2

    public ScenarioInfo CurrentScenarioInfo { get; set; }

    void Init(string scenarioName)
    {
        ScenarioName = scenarioName;
        Progress = 0;
        CompleteCount = 0;
        _checkComplete = false;
        PassSpeech = false;
        CurrentScenarioInfo = Managers.Data.ScenarioData[ScenarioName][Progress];
        ScenarioAssist.transform.GetChild(2).gameObject.SetActive(_scenarioHint);


        bool patientAdded = AddNPC("환자", WaitingArea);
        bool transportOfficerAdded = AddNPC("이송요원", WaitingArea);
        bool securityOfficer1Added = AddNPC("보안요원1", WaitingArea);
        bool securityOfficer2Added = AddNPC("보안요원2", WaitingArea);
        bool cleaner1Added = AddNPC("미화1", WaitingArea);
        bool cleaner2Added = AddNPC("미화2", WaitingArea);
    }

    public void Reset()
    {
        PassSpeech = false;
        MyAction = null;
        Targets.Clear();
        Item = null;
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
        if(_doingScenario == false)
        {
            _doingScenario = true;
            Managers.Instance.StartCoroutine(CoScenario(scenarioName));
        }
    }

    IEnumerator CoScenarioStep(int progress)
    {
        Managers.STT.STTStreamingText.RegisterCommand(CurrentScenarioInfo.DetailHint, CurrentScenarioInfo.Position == Managers.Object.MyPlayer.Position);
        if (Managers.Object.MyPlayer.Position == CurrentScenarioInfo.Position)
        {
            UpdateScenarioAssist($"{CurrentScenarioInfo.Hint}");
            Managers.Instance.StartCoroutine(CoCheckAction());
            yield return new WaitUntil(() => CompleteCount >= 1);

            if (CurrentScenarioInfo.Confirm != null)
            {
                GameObject go = Managers.UI.CreateUI("ScenarioCheckPopup");
                go.GetComponent<ScenarioCheckPopup>().UpdateText(CurrentScenarioInfo.Confirm);

                yield return new WaitUntil(() => PopupConfirm != 0);

                if(PopupConfirm == 1)
                {
                    PopupConfirm = 0;
                    Managers.UI.CreateSystemPopup("WarningPopup", "<color=#00ff00>시나리오를 통과했습니다.</color>");
                }
                else if(PopupConfirm == 2)
                {
                    PopupConfirm = 0;
                    CompleteCount = 0;
                    yield return Managers.Instance.StartCoroutine(CoScenarioStep(progress));
                    yield break;
                }
            }
        }
        else
        {
            UpdateScenarioAssist(CurrentScenarioInfo.Position + " 플레이어가 시나리오를 진행 중 입니다...");
        }
        SendComplete();

        yield return new WaitUntil(() => Progress == progress);
    }

    IEnumerator CoScenario(string scenarioName)
    {
        Managers.UI.CreateSystemPopup("PopupNotice", $"{scenarioName} 시나리오를 시작합니다.");
        yield return new WaitForSeconds(3.0f);

        Managers.UI.CreateSystemPopup("PopupNotice", $"사랑합니다.\n지금부터 신종감염병 대응 모의 훈련을 시작하고자 하오니 환자 및 보호자께서는 동요하지 마시기 바랍니다.\n모의 훈련 요원들은 지금부터 훈련을 시작하도록 하겠습니다.");
        yield return new WaitForSeconds(3.0f);

        Init(scenarioName);

        switch (scenarioName)
        {
            case "엠폭스":
                NPCs["환자"].GetComponent<NavMeshAgent>().enabled = false; 
                NPCs["환자"].FreezePosition();

                //환자를 침대 자식오브젝트로 설정, 환자를 침대에 눕힘
                NPCs["환자"].transform.SetParent(GameObject.Find("move_bed").transform);
                NPCs["환자"].transform.localPosition = Patientlying;
                NPCs["환자"].transform.localEulerAngles = new Vector3(0, 270, 0);
                NPCs["환자"].SetState(CreatureState.LyingIdle);

                Managers.UI.ChangeChatBubble(NPCs["환자"].transform, "선생님 방금 가족 중에 한명이 보건소로부터 엠폭스 확진받았다고 연락을 받아서요.\n저도 곧 보건소로부터 연락올거라고 합니다.");
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(1));
                Managers.UI.ChangeChatBubble(NPCs["환자"].transform, "이관리 980421 입니다.\n같이 살고있어요.");
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(2));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(3));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(4));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(5));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(6));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(7));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(8));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(9));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(10));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(11));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(12));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(13));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(14));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(15));
                //환자 음압격리실로 이송
                {
                    NPCs["보안요원1"].Use("Mask");
                    NPCs["보안요원2"].Use("Mask");
                    NPCs["미화1"].Use("Mask");
                    NPCs["미화2"].Use("Mask");

                    Managers.Object.ChangeModel(NPCs["이송요원"], "ProtectedGear");
                    Managers.Object.ChangeModel(NPCs["보안요원1"], "ProtectedGear");
                    Managers.Object.ChangeModel(NPCs["보안요원2"], "ProtectedGear");

                    Managers.UI.ChangeChatBubble(NPCs["보안요원1"].transform, "격리 환자 이송 중입니다.\n통제에 따라주세요");
                    Managers.UI.ChangeChatBubble(NPCs["보안요원2"].transform, "격리 환자 이송 중입니다.\n통제에 따라주세요");

                    NPCs["이송요원"].Teleport(WaitingArea);
                    NPCs["보안요원1"].Teleport(Entrance);
                    NPCs["보안요원1"].AddOrder(NPCs["보안요원1"].CoGoDestination(BlockingPoint1));
                    NPCs["보안요원1"].AddOrder(NPCs["보안요원1"].CoSetForward(-Vector3.right));
                    NPCs["보안요원1"].AddOrder(NPCs["보안요원1"].CoSetState(CreatureState.Blocking));
                    yield return new WaitForSeconds(1.0f);

                    NPCs["보안요원2"].Teleport(Entrance);
                    NPCs["보안요원2"].AddOrder(NPCs["보안요원2"].CoGoDestination(BlockingPoint2));
                    NPCs["보안요원2"].AddOrder(NPCs["보안요원2"].CoSetForward(-Vector3.right));
                    NPCs["보안요원2"].AddOrder(NPCs["보안요원2"].CoSetState(CreatureState.Blocking));
                    yield return new WaitForSeconds(1.0f);

                    yield return new WaitUntil(() => (NPCs["보안요원2"].transform.position - BlockingPoint2).magnitude < 1);

                    NPCs["이송요원"].Teleport(Entrance);
                    NPCs["이송요원"].AddOrder(NPCs["이송요원"].CoGoDestination(MovePosition));

                    yield return new WaitUntil(() => (NPCs["이송요원"].transform.position - MovePosition).magnitude < 1);

                    GameObject bed = GameObject.Find("move_bed");
                    bed.transform.SetParent(NPCs["이송요원"].transform);
                    NPCs["이송요원"].transform.GetChild(1).localPosition = new Vector3(0, 0, 1.2f);
                    NPCs["이송요원"].transform.GetChild(1).localEulerAngles = new Vector3(0, -90, 0);
                    NPCs["이송요원"].AddOrder(NPCs["이송요원"].CoGoDestination_Animation(IsolationArea, CreatureState.Push));
                    NPCs["이송요원"].ChangeSpeed(2f);
                    GameObject go1 = Managers.Resource.Instantiate("System/ControlSphere", NPCs["보안요원1"].transform);
                    GameObject go2 = Managers.Resource.Instantiate("System/ControlSphere", NPCs["보안요원2"].transform);
                    yield return new WaitForSeconds(1.0f);

                    NPCs["미화1"].Teleport(Entrance);
                    NPCs["미화1"].AddOrder(NPCs["미화1"].CoGoDestination(OABed));
                    NPCs["미화1"].AddOrder(NPCs["미화1"].CoUse("WetMop"));
                    yield return new WaitForSeconds(1.0f);

                    NPCs["미화2"].Teleport(Entrance);
                    NPCs["미화2"].AddOrder(NPCs["미화2"].CoGoDestination(OATable));
                    NPCs["미화2"].AddOrder(NPCs["미화2"].CoUse("TissueBox", () => NPCs["미화2"].SetForward(NPCs["미화2"].transform.forward)));
                    yield return new WaitForSeconds(1.0f);

                    yield return new WaitUntil(() => NPCs["이송요원"].Place == "음압격리실");

                    //이송된 환자 음압격리실에 세팅
                    NPCs["환자"].SetState(CreatureState.LyingIdle);
                    bed.transform.SetParent(null);
                    bed.transform.position = Define.IABed;
                    bed.transform.eulerAngles = new Vector3(0, 90, 0);

                    //환자 이송이 끝나면 모든 NPC 상태 초기화, 출입구로 이동 후 퇴장 (환자 제외)
                    NPCs["이송요원"].ResetSpeed();
                    StopAllNPC(NPCs["환자"]);
                    SetStateAllNPC(CreatureState.Idle, NPCs["환자"]);
                    MoveAllNPC(Entrance, NPCs["환자"]);
                    WarpAllNPC(WaitingArea, NPCs["환자"]);

                    Managers.Resource.Destroy(go1);
                    Managers.Resource.Destroy(go2);
                }
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(16));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(17));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(18));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(19));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(20));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(21));
                Managers.UI.ChangeChatBubble(NPCs["환자"].transform, "이감염 001218년생 입니다.");
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(22));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(23));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(24));
                Managers.UI.ChangeChatBubble(NPCs["환자"].transform, "아니요. 딱히 없었어요.");
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(25));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(26));
                Managers.UI.ChangeChatBubble(NPCs["환자"].transform, "이감염 001218년생 입니다.");
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(27));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(28));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(29));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(30));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(31));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(32));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(33));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(34));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(35));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(36));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(37));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(38));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(39));
                Managers.UI.ChangeChatBubble(NPCs["환자"].transform, "이감염 001218년생 입니다.");
                //퀴즈 
                if (Managers.Scenario.CurrentScenarioInfo.Position == "응급센터 간호사2")
                    yield return Managers.Instance.StartCoroutine(Managers.Quiz.QuizUI(40));

                yield return Managers.Instance.StartCoroutine(CoScenarioStep(40));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(41));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(42));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(43));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(44));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(45));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(46));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(47));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(48));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(49));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(50));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(51));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(52));
                Managers.UI.ChangeChatBubble(NPCs["환자"].transform, "이감염 001218년생 입니다.");
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(53));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(54));
                break;
        }

        UpdateScenarioAssist("시나리오를 완료하셨습니다.");
        Managers.UI.CreateSystemPopup("PopupNotice", $"{scenarioName} 시나리오를 완료하셨습니다.");
    }

    #endregion

    #region 시나리오 진행 관련 기능

    //현재 진행된 시나리오에 대하여 다른 플레이어들이 확인할 수 있도록 말풍선, 메시지 등의 상황을 업데이트 해주는 함수
    void UpdateSituation()
    {
        switch (CurrentScenarioInfo.Action)
        {
            case "Call":
                Managers.Phone.Device.SendMessage(CurrentScenarioInfo.Position, CurrentScenarioInfo.DetailHint, CurrentScenarioInfo.Targets);
                break;
        }
    }

    //서버로부터 다음 시나리오 진행도를 받으면, 시나리오 상황 업데이트 및 변수 초기화 등 실행
    public void NextProgress(int progress)
    {
        ClearNPCBubble();
        UpdateSituation();

        Progress = progress;
        CompleteCount = 0;
        _checkComplete = false;
        Reset();
        CurrentScenarioInfo = Managers.Data.ScenarioData[ScenarioName][Progress];
        _routine = null;


    }

    //화면 상단에 시나리오 진행 관련 힌트를 주는 UI 업데이트
    GameObject Hint;

    public void UpdateScenarioAssist(string state, string position = null)
    {
        if (position != null)
            ScenarioAssist.transform.GetChild(0).GetComponent<TMP_Text>().text = position;

        ScenarioAssist.transform.GetChild(1).GetComponent<TMP_Text>().text = state;

        if (Hint == null)
            Hint = Util.FindChildByName(ScenarioAssist, "Hint");

        if (CurrentScenarioInfo == null)
            return;


        // 애초에 YudoLine을 비활성화 상태로 유지
        if (GameScene.YudoLine != null)
        {
            GameScene.YudoLine.SetActive(false);  // Progress 상관없이 기본적으로 비활성화
        }

        // 특정 Progress 값에서만 YudoLine을 활성화
        if (ShouldShowYudoLine(Progress))
        {
            if (GameScene.YudoLine != null)
            {
                GameScene.YudoLine.SetActive(true);  // 특정 Progress에서만 활성화
            }
        }

        Util.FindChildByName(Hint, "HintSpeech").GetComponent<TMP_Text>().text = CurrentScenarioInfo.DetailHint;
    }

    #endregion

    #region 시나리오 검증 관련 기능

    IEnumerator CoCheckAction()
    { 
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
        if (!CheckPlace())
            return false;

        if (MyAction == null)
            return false;

        if (!string.IsNullOrEmpty(CurrentScenarioInfo.Item))
        {
            if (string.IsNullOrEmpty(Item))
                return false;
        }

        if (CurrentScenarioInfo.Action == "Tell" || CurrentScenarioInfo.Action == "Call")
            if (PassSpeech == false)
                return false;

        if (CurrentScenarioInfo.Targets.Count > 0)
            if (Targets.Count == 0)
                return false;

        return true;
    }

    bool CheckAction()
    {
        if (MyAction != CurrentScenarioInfo.Action)
        {
            Managers.UI.CreateSystemPopup("WarningPopup", "<color=#ff0000>올바른 행동을 수행하지 않았습니다.</color>");
            Reset();
            return false;
        }

        if (!CheckTarget())
        {
            Managers.UI.CreateSystemPopup("WarningPopup", "<color=#ff0000>대상이 올바르지 않습니다.</color>");
            Reset();
            return false;
        }

        if (!string.IsNullOrEmpty(CurrentScenarioInfo.Item))
        {
            if (Item != CurrentScenarioInfo.Item)
            {
                Managers.UI.CreateSystemPopup("WarningPopup", "<color=#ff0000>현재 상황에 알맞게 장비를 착용/해제 하지 않았습니다.</color>");
                Reset();
                return false;
            }
        }

        if (!(CurrentScenarioInfo.Action == "Tell" || CurrentScenarioInfo.Action == "Call"))
            return true;

        if (!PassSpeech)
        {
            Managers.UI.CreateSystemPopup("WarningPopup", "<color=#ff0000>상황에 맞지 않는 대화이거나 대화 장소가 잘못되었습니다.</color>");
            Reset();
            return false;
        }

        return true;
    }

    bool CheckTarget()
    {
        if (Targets.Count < CurrentScenarioInfo.Targets.Count)
            return false;

        foreach(var target in CurrentScenarioInfo.Targets)
        {
            if (!Targets.Contains(target))
                return false;
        }

        return true;
    }

    public bool CheckPlace()
    {
        if (CurrentScenarioInfo == null)
            return true;

        if (CurrentScenarioInfo.Position != Managers.Object.MyPlayer.Position)
            return true;

        //시나리오 진행 장소가 정해지지 않은 경우 통과
        if (string.IsNullOrEmpty(CurrentScenarioInfo.Place))
            return true;

        //장비 착용과 관련된 시나리오가 아니면 통과
        if(!(CurrentScenarioInfo.Action == "Equip" || CurrentScenarioInfo.Action == "UnEquip"))
            return true;

        //시나리오 진행 장소가 정해져있는 경우, 현재 내가 해당 장소에 있는지 확인
        if (CurrentScenarioInfo.Place == Managers.Object.MyPlayer.Place)
            return true;
        else
            return false;
    }

    public float CheckKeywords(ref string message)
    {
        if (string.IsNullOrEmpty(message))
            return 0;

        float count = 0;
        List<string> needKeywords = new List<string>();

        foreach (var keyword in CurrentScenarioInfo.STTKeywords)
        {
            if (message.Contains(keyword))
            {
                count += 1;
                message = message.Replace(keyword, $"<color=#00ff00>{keyword}</color>");
            }
            else
            {
                needKeywords.Add(keyword);
            }
        }

        if(needKeywords.Count > 0)
        {
            message += "\n<color=#ff0000>필요한 키워드 : ";

            foreach (var keyword in needKeywords)
            {
                message += $"{keyword} ";
            }
            message += "</color>";
        }

        return count / (float)CurrentScenarioInfo.STTKeywords.Count;
    }

    #endregion

    #region 시나리오 NPC 제어 기능

    bool AddNPC(string position, Vector3 spawnPoint)
    {
        GameObject go = Managers.Resource.Instantiate($"Creatures/NPC/{position}");

        if (go == null)
        {
            Debug.LogError($"Can't find {position} NPC prefab");
            return false;
        }

        NPCController nc = go.GetComponent<NPCController>();
        nc.Position = position;
        nc.Teleport(spawnPoint);

        NPCs.Add(nc.Position, nc);

        return true;
    }

    void ClearNPCBubble()
    {
        foreach (var npc in NPCs.Values)
        {
            Managers.UI.InvisibleBubble(npc.transform);
        }
    }

    void StopAllNPC(NPCController except = null)
    {
        foreach (var npc in NPCs.Values)
        {
            if (npc == except)
                continue;

            npc.StopOrder();
        }
    }

    void SetStateAllNPC(CreatureState state, NPCController except = null)
    {
        foreach (var npc in NPCs.Values)
        {
            if (npc == except)
                continue;

            npc.SetState(state);
        }
    }

    void MoveAllNPC(Vector3 des, NPCController except = null)
    {
        foreach (var npc in NPCs.Values)
        {
            if (npc == except)
                continue;

            npc.AddOrder(npc.CoGoDestination(des));
        }
    }

    void WarpAllNPC(Vector3 des, NPCController except = null)
    {
        foreach (var npc in NPCs.Values)
        {
            if (npc == except)
                continue;

            npc.AddOrder(npc.CoTeleport(des));
        }
    }

    #endregion

    bool ShouldShowYudoLine(int progress)
    {
        int[] showYudoLineProgress = { 20, 25, 38, 51 };

        return Array.Exists(showYudoLineProgress, p => p == progress);
    }
}
