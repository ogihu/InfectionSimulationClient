using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;
using static Define;
using static Google.Cloud.Speech.V1.LanguageCodes;

public class ScenarioManager
{
    public int CompleteCount { get; set; }

    public string ScenarioName { get; set; }
    public int Progress { get; set; }
    public string Item { get; set; }

    public int Score = 100;

    public Dictionary<string, NPCController> NPCs = new Dictionary<string, NPCController>();
    public Dictionary<string, PlayerNPCController> PlayerNPCs = new Dictionary<string, PlayerNPCController>();
    Coroutine _coPlayerNPC;

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

            return _scenarioAssist;
        }
    }

    public List<GameObject> InteractableObjectsList = new List<GameObject>();
    List<GameObject> ObjectIndicators = new List<GameObject>();
    GameObject ObjectIndicator { get; set; }

    GameObject _myPlace;
    public GameObject MyPlace
    {
        get
        {
            if (_myPlace == null)
                _myPlace = GameObject.Find("MyPlace");

            if (_myPlace == null)
                _myPlace = Managers.UI.CreateUI("MyPlace");

            return _myPlace;
        }
    }
    string _place;

    public bool AllPlayerNPCCompleted;
    public bool TTSPlaying = false;

    public Navigation Navigation { get; set; }

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
    public List<string> TakedLesion { get; set; } = new List<string>();

    #endregion

    private bool _checkComplete;    //이미 Complete 패킷을 보냈는지 확인, 서버에서 시나리오는 진행시키면 (NextProcess 패킷을 받으면) false로 전환해야 함.
    public Coroutine _routine;
    public bool _doingScenario = false;
    public int PopupConfirm { get; set; }   //평상시에는 0, CheckPopup에서 확인을 선택했으면 1, 취소를 선택했으면 2
    IEnumerator _scenarioCoroutine;

    public ScenarioInfo CurrentScenarioInfo { get; set; }

    void Init(string scenarioName, S_StartScenario packet)
    {
        PassUICheck = true;
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
        bool securityOfficer3Added = AddNPC("보안요원3", WaitingArea);
        bool securityOfficer4Added = AddNPC("보안요원4", WaitingArea);
        bool cleaner1Added = AddNPC("미화1", WaitingArea);
        bool cleaner2Added = AddNPC("미화2", WaitingArea);

        if(packet.LackPositions.Count > 0)
        {
            foreach(var position in packet.LackPositions)
            {
                AddPlayerNPC(position);
            }
        }

        GameObject objects = GameObject.Find("InteractableObjects");
        int childCount = objects.transform.childCount;
        for(int i = 0; i < childCount; i++)
        {
            InteractableObjectsList.Add(objects.transform.GetChild(i).gameObject);
        }

        ObjectIndicator = Managers.Resource.Load<GameObject>("Prefabs/System/ObjectIndicator");
    }
    
    public void Reset()
    {
        PassSpeech = false;
        MyAction = null;
        Targets.Clear();
        Item = null;
    }

    #region 시나리오 패킷 관련 기능

    public void SendScenarioInfo()
    {
        C_StartScenario scenarioPacket = new C_StartScenario();
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
    public void StartScenario(string scenarioName, S_StartScenario packet)
    {
        if(_doingScenario == false)
        {
            _doingScenario = true;
            _scenarioCoroutine = CoScenario(scenarioName, packet);
            Managers.Instance.StartCoroutine(_scenarioCoroutine);
        }
    }

    #region 시나리오 보호구 UI 띄우는 코드
    public bool PassUICheck = true;
    Coroutine PassUI;
    public GameObject WearUI1 = null;
    public GameObject WearUI2 = null;

    public GameObject SpecimeCollection1 = null;
    public GameObject SpecimeCollection2 = null;

    GameObject popup = null;
    bool UIChckStart = true;
    public bool State_Image = false;

    void Cursor_activation(bool check = false)
    {
        State_Image = check;
    }

    IEnumerator WearingUICheck()
    {
        string a;
        if (CurrentScenarioInfo.Action == "EquipImage")
            a = "착의법 ";
        else
            a = "탈의법 ";
        popup = Managers.UI.CreateUI("PopupNotice");
        PassUICheck = false;
        int m = 0;
        for (int i = 3; i > 0; i--)
        {
            if (popup == null)
                yield break;
            popup.transform.GetChild(0).GetComponent<TMP_Text>().alignment = TextAlignmentOptions.Center;   //중앙정렬
            popup.transform.GetChild(0).GetComponent<TMP_Text>().text = i.ToString() + "초 뒤, 개인보호구 "+a+"안내 이미지가 제공됩니다.";
            yield return new WaitForSeconds(1f); // 1초 대기
        }
        Managers.UI.DestroyUI(popup);
        if (CurrentScenarioInfo.Action == "EquipImage")
        {
            WearUI1 = Managers.UI.CreateUI("WearingWay");
            if (WearUI1 != null)
            {
                Transform coverImagesParent = WearUI1.transform.Find("CoverImage");
                if (coverImagesParent != null)
                {
                    yield return Util.FadeOutCoverImages(coverImagesParent, 1f); 
                }
            }
                WearUI2 = Managers.UI.CreateUI("Final_Wearing_Image");
            UIChckStart = false;
            WearUI2.SetActive(false);
            m = 20;
        }
        else if (CurrentScenarioInfo.Action == "UnEquipImage")
        {
            WearUI1 = Managers.UI.CreateUI("LayOutWay");
            if (WearUI1 != null)
            {
                Transform coverImagesParent = WearUI1.transform.Find("CoverImage");
                if (coverImagesParent != null)
                {
                    yield return Util.FadeOutCoverImages(coverImagesParent, 1f); 
                }
            }
            UIChckStart = false;
            m = 10;
        }

        Cursor_activation(true);
        State_Image = true;
        int k = 0;
        for (int i = 0; i < m; i++)
        {
            if ((CurrentScenarioInfo.Action == "EquipImage" && i == 10) || WearUI1 == null )
            {
                if( k == 0 )
                {
                    i = 10;
                    k = 1;
                    Managers.UI.DestroyUI(WearUI1);
                    WearUI2.SetActive(true);
                }
            }
            if (WearUI1 == null && WearUI2 == null)
                break;
            yield return new WaitForSeconds(1f);
        }

        State_Image = false;
        if (WearUI1 != null)
            Managers.UI.DestroyUI(WearUI1);
        if(WearUI2 != null)
            Managers.UI.DestroyUI(WearUI2);
        CompleteCount++;
    }

    IEnumerator UIcheck()
    {
        while (!PassUICheck)
        {
            if (!UIChckStart)
            {
                if ((WearUI1 == null && WearUI2 == null )&&(SpecimeCollection1 == null && SpecimeCollection2 == null))

                {
                    Managers.Instance.StopCoroutine(PassUI);
                    Managers.Object.MyPlayer.GetComponent<MyPlayerController>().enabled = true;
                    Cursor_activation();
                    PassUICheck = true;
                    CompleteCount++;
                    PassUI = null;
                }
            }
            yield return null;
        }
    }
    IEnumerator SpecimeCollectionUICheck()
    {
        // 카운트다운 메시지 표시
        popup = Managers.UI.CreateUI("PopupNotice");
        PassUICheck = false;

        for (int i = 3; i > 0; i--)
        {
            if (popup == null)
                yield break;
            popup.transform.GetChild(0).GetComponent<TMP_Text>().alignment = TextAlignmentOptions.Center;
            popup.transform.GetChild(0).GetComponent<TMP_Text>().text = i.ToString() + "초 뒤, 검체 채취 이미지가 제공됩니다.";
            yield return new WaitForSeconds(1f);
        }
        Managers.UI.DestroyUI(popup);
        State_Image = true;
        SpecimeCollection1 = Managers.UI.CreateUI("SpecimeCollection1");
        SpecimeCollection2 = Managers.UI.CreateUI("SpecimeCollection2");
        UIChckStart = false;
        SpecimeCollection2.SetActive(false);

        int k = 0;
        for (int i = 0; i < 20; i++)
        {
            if ((CurrentScenarioInfo.Action == "SCImage" && i == 10) || SpecimeCollection1 == null)
            {
                if (k == 0)
                {
                    k = 1;
                    i = 10;
                    Managers.UI.DestroyUI(SpecimeCollection1);
                    SpecimeCollection2.SetActive(true);
                }
            }
            if (SpecimeCollection1 == null && SpecimeCollection2 == null)
                break;
            yield return new WaitForSeconds(1f);
        }
        // UI 소멸 및 시나리오 완료 처리
        if (SpecimeCollection1 != null)
            Managers.UI.DestroyUI(SpecimeCollection1);
        if (SpecimeCollection2 != null)
            Managers.UI.DestroyUI(SpecimeCollection2);
        State_Image = false;
        CompleteCount++;

    }
    IEnumerator WearingUI()
    {
        UIChckStart = true;
        PassUICheck = false;
        PassUI = Managers.Instance.StartCoroutine(WearingUICheck());
        yield return Managers.Instance.StartCoroutine(UIcheck());

    }
    IEnumerator SpecimeCollectionUI()
    {
        UIChckStart = true;
        PassUICheck = false;
        PassUI = Managers.Instance.StartCoroutine(SpecimeCollectionUICheck());
        yield return Managers.Instance.StartCoroutine(UIcheck());
    }
    #endregion

    IEnumerator CoScenarioStep(int progress)
    {
        Managers.Keyword.CanOpen = true;
        Managers.Keyword.CanClose = true;

        if (TTSPlaying)
        {
            if (Managers.TTS.Speaker != null)
            {
                if (Managers.TTS.Speaker.gameObject.layer == LayerMask.NameToLayer("NPC"))
                    UpdateScenarioAssist(Managers.TTS.Speaker.Position + " NPC가 시나리오를 진행 중 입니다...");
                else if(Managers.TTS.Speaker.gameObject.layer == LayerMask.NameToLayer("Player"))
                    UpdateScenarioAssist(Managers.TTS.Speaker.Position + " 플레이어가 시나리오를 진행 중 입니다...");
                else if (Managers.TTS.Speaker.gameObject.layer == LayerMask.NameToLayer("MyPlayer"))
                    UpdateScenarioAssist("캐릭터가 대화를 진행 중입니다...");
            }
        }
        yield return new WaitUntil(() => !TTSPlaying);

        if (PlayerNPCs.Count > 0)
        {
            AllPlayerNPCCompleted = false;
            if (PlayerNPCs.Count > 0)
            {
                foreach (var npc in PlayerNPCs.Values)
                {
                    npc.PassScenario = false;
                }
            }
            _coPlayerNPC = Managers.Instance.StartCoroutine(CoInitPlayerNPCs());
        }
        else
        {
            AllPlayerNPCCompleted = true;
        }

        UpdateMyPlace(); 
        Managers.STT.STTStreamingText.RegisterCommand(CurrentScenarioInfo.DetailHint, CurrentScenarioInfo.Position == Managers.Object.MyPlayer.Position);
        Managers.Setting.SceneStartMicCheck();

        UpdateObjectIndicator(CurrentScenarioInfo.ObjectIndicator);

        if (Managers.Object.MyPlayer.Position == CurrentScenarioInfo.Position)
        {
            UpdateScenarioAssist($"{CurrentScenarioInfo.Hint}");

            if (CurrentScenarioInfo.Action == "Quiz")
            {
                Managers.Instance.StartCoroutine(Managers.Quiz.CoQuizCount(3));
            }
            else if (CurrentScenarioInfo.Action == "LinkQuiz")
            {
                Managers.Instance.StartCoroutine(Managers.Quiz.CoQuizCount(3, QuizManager.QuizType.Link));
            }

            if (CurrentScenarioInfo.Action == "EquipImage" || CurrentScenarioInfo.Action == "UnEquipImage")
                Managers.Instance.StartCoroutine(WearingUI());

            if (CurrentScenarioInfo.Action == "SCImage")
            {
                Managers.Instance.StartCoroutine(SpecimeCollectionUI());
            }
            
            Managers.Instance.StartCoroutine(CoCheckAction());
            yield return new WaitUntil(() => CompleteCount >= 1);


            if (CurrentScenarioInfo.Confirm != null)
            {
                GameObject go = Managers.UI.CreateUI("ScenarioCheckPopup");
                go.GetComponent<ScenarioCheckPopup>().UpdateText(CurrentScenarioInfo.Confirm);

                yield return new WaitUntil(() => PopupConfirm != 0);

                if (PopupConfirm == 1)
                {
                    PopupConfirm = 0;
                    Managers.UI.CreateSystemPopup("WarningPopup", "시나리오를 통과했습니다.", UIManager.NoticeType.Info);
                }
                else if (PopupConfirm == 2)
                {
                    PopupConfirm = 0;
                    CompleteCount = 0;
                    yield return Managers.Instance.StartCoroutine(CoScenarioStep(progress));
                    yield break;
                }
            }
            else
            {
                Managers.UI.CreateSystemPopup("WarningPopup", "시나리오를 통과했습니다.", UIManager.NoticeType.Info);
            }
        }
        else if (CurrentScenarioInfo.Position == "NPC")
        {
            UpdateScenarioAssist("NPC가 시나리오를 진행 중 입니다...");
        }
        else
        {
            if(Managers.Object.Characters.TryGetValue(CurrentScenarioInfo.Position, out BaseController bc))
            {
                if(bc.gameObject.layer == LayerMask.NameToLayer("NPC"))
                    UpdateScenarioAssist(CurrentScenarioInfo.Position + " NPC가 시나리오를 진행 중 입니다...");
                else
                    UpdateScenarioAssist(CurrentScenarioInfo.Position + " 플레이어가 시나리오를 진행 중 입니다...");
            }
            else
            {
                UpdateScenarioAssist(CurrentScenarioInfo.Position + " 플레이어가 시나리오를 진행 중 입니다...");
            }
        }

        yield return _coPlayerNPC;
        yield return new WaitUntil(() => AllPlayerNPCCompleted);

        if (TTSPlaying)
        {
            if (Managers.TTS.Speaker != null)
            {
                if (Managers.TTS.Speaker.gameObject.layer == LayerMask.NameToLayer("NPC"))
                    UpdateScenarioAssist(Managers.TTS.Speaker.Position + " NPC가 시나리오를 진행 중 입니다...");
                else if (Managers.TTS.Speaker.gameObject.layer == LayerMask.NameToLayer("Player"))
                    UpdateScenarioAssist(Managers.TTS.Speaker.Position + " 플레이어가 시나리오를 진행 중 입니다...");
                else if (Managers.TTS.Speaker.gameObject.layer == LayerMask.NameToLayer("MyPlayer"))
                    UpdateScenarioAssist("캐릭터가 대화를 진행 중입니다...");
            }
        }
        yield return new WaitUntil(() => !TTSPlaying);

        SendComplete();

        yield return new WaitUntil(() => Progress == progress);
        yield break;
    }

    IEnumerator CoScenario(string scenarioName, S_StartScenario packet)
    {
        Managers.UI.CreateSystemPopup("PopupNotice", $"{scenarioName} 시나리오를 시작합니다.", UIManager.NoticeType.None);
        yield return new WaitForSeconds(3.0f);

        Managers.UI.CreateSystemPopup("PopupNotice", $"사랑합니다.\n지금부터 신종감염병 대응 모의 훈련을 시작하고자 하오니 환자 및 보호자께서는 동요하지 마시기 바랍니다.\n모의 훈련 요원들은 지금부터 훈련을 시작하도록 하겠습니다.", UIManager.NoticeType.None);
        yield return new WaitForSeconds(3.0f);

        Init(scenarioName, packet);

        #region 시나리오 진행

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
                Managers.UI.ChangeChatBubble(NPCs["환자"].transform, "이관리 9 8 0 4 2 1 입니다.\n같이 살고있어요.");
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
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(16));
                #region 환자 격리실 이송
                {
                    NPCs["미화1"].Use("Mask");
                    NPCs["미화2"].Use("Mask");

                    NPCs["이송요원"].Teleport(WaitingArea);
                    NPCs["보안요원1"].Teleport(WaitingArea);
                    NPCs["보안요원2"].Teleport(WaitingArea);
                    NPCs["보안요원3"].Teleport(WaitingArea);
                    NPCs["보안요원4"].Teleport(WaitingArea);

                    Managers.UI.ChangeChatBubble(NPCs["보안요원1"].transform, "격리 환자 이송 중입니다.\n통제에 따라주세요.", false);
                    Managers.UI.ChangeChatBubble(NPCs["보안요원2"].transform, "격리 환자 이송 중입니다.\n통제에 따라주세요.");
                    Managers.UI.ChangeChatBubble(NPCs["보안요원3"].transform, "격리 환자 이송 중입니다.\n통제에 따라주세요.", false);
                    Managers.UI.ChangeChatBubble(NPCs["보안요원4"].transform, "격리 환자 이송 중입니다.\n통제에 따라주세요.", false);

                    NPCs["보안요원2"].Teleport(Entrance);
                    NPCs["보안요원2"].AddOrder(NPCs["보안요원2"].CoGoDestination(BlockingPoint3));
                    NPCs["보안요원2"].AddOrder(NPCs["보안요원2"].CoSetForward(-Vector3.right));
                    NPCs["보안요원2"].AddOrder(NPCs["보안요원2"].CoSetState(CreatureState.Blocking));
                    yield return new WaitForSeconds(1.0f);

                    NPCs["보안요원3"].Teleport(Entrance);
                    NPCs["보안요원3"].AddOrder(NPCs["보안요원3"].CoGoDestination(BlockingPoint2));
                    NPCs["보안요원3"].AddOrder(NPCs["보안요원3"].CoSetForward(-Vector3.right));
                    NPCs["보안요원3"].AddOrder(NPCs["보안요원3"].CoSetState(CreatureState.Blocking));
                    yield return new WaitForSeconds(1.0f);

                    NPCs["보안요원4"].Teleport(Entrance);
                    NPCs["보안요원4"].AddOrder(NPCs["보안요원4"].CoGoDestination(BlockingPoint1));
                    NPCs["보안요원4"].AddOrder(NPCs["보안요원4"].CoSetForward(-Vector3.right));
                    NPCs["보안요원4"].AddOrder(NPCs["보안요원4"].CoSetState(CreatureState.Blocking));
                    yield return new WaitForSeconds(1.0f);

                    yield return new WaitUntil(() => (NPCs["보안요원2"].transform.position - BlockingPoint3).magnitude < 1);

                    GameObject restrictedArea = Managers.Resource.Instantiate("System/OAToIACollider");

                    NPCs["보안요원1"].Teleport(Entrance);
                    NPCs["보안요원1"].AddOrder(NPCs["보안요원1"].CoGoDestination(MovePosition));
                    yield return new WaitForSeconds(3.0f);

                    NPCs["이송요원"].Teleport(Entrance);
                    NPCs["이송요원"].AddOrder(NPCs["이송요원"].CoGoDestination(MovePosition));

                    yield return new WaitUntil(() => (NPCs["보안요원1"].transform.position - MovePosition).magnitude < 0.3f);

                    NPCs["보안요원1"].AddOrder(NPCs["보안요원1"].CoGoDestination(IsolationArea));
                    NPCs["보안요원1"].ChangeSpeed(3f);

                    yield return new WaitUntil(() => (NPCs["이송요원"].transform.position - MovePosition).magnitude < 0.3f);

                    GameObject bed = GameObject.Find("move_bed");
                    bed.transform.SetParent(NPCs["이송요원"].transform);
                    NPCs["이송요원"].transform.GetChild(1).localPosition = new Vector3(0, 0, 1.2f);
                    NPCs["이송요원"].transform.GetChild(1).localEulerAngles = new Vector3(0, -90, 0);
                    NPCs["이송요원"].AddOrder(NPCs["이송요원"].CoGoDestination_Animation(IsolationArea, CreatureState.Push));
                    NPCs["이송요원"].ChangeSpeed(3f);
                    GameObject go1 = Managers.Resource.Instantiate("System/ControlSphere", NPCs["보안요원1"].transform);
                    GameObject go2 = Managers.Resource.Instantiate("System/ControlSphere", NPCs["보안요원2"].transform);
                    GameObject go3 = Managers.Resource.Instantiate("System/ControlSphere", NPCs["보안요원3"].transform);
                    GameObject go4 = Managers.Resource.Instantiate("System/ControlSphere", NPCs["보안요원4"].transform);
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
                    NPCs["보안요원1"].ResetSpeed();
                    StopAllNPC(NPCs["환자"]);
                    SetStateAllNPC(CreatureState.Idle, NPCs["환자"]);
                    MoveAllNPC(Entrance, NPCs["환자"]);
                    WarpAllNPC(WaitingArea, NPCs["환자"]);

                    Managers.Resource.Destroy(go1);
                    Managers.Resource.Destroy(go2);
                    Managers.Resource.Destroy(go3);
                    Managers.Resource.Destroy(go4);
                    Managers.Resource.Destroy(restrictedArea);
                }
                #endregion
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(17));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(18));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(19));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(20));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(21));
                Managers.UI.ChangeChatBubble(NPCs["환자"].transform, "이감염 0 0 1 2 1 8 년생 입니다.");
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(22));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(23));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(24));
                Managers.UI.ChangeChatBubble(NPCs["환자"].transform, "아니요. 딱히 없었어요.");
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(25));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(26));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(27));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(28));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(29));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(30));
                Managers.UI.ChangeChatBubble(NPCs["환자"].transform, "이감염 0 0 1 2 1 8 년생 입니다.");
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(31));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(32));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(33));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(34));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(35));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(36));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(37));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(38));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(39));
                Managers.UI.ChangeChatBubble(NPCs["환자"].transform, "이감염 0 0 1 2 1 8 년생 입니다.");
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(40));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(41));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(42));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(43));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(44));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(45));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(46));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(47));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(48));
                Managers.UI.ChangeChatBubble(NPCs["환자"].transform, "이감염  0 0 1 2 1 8 년생 입니다.");
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(49));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(50));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(51));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(52));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(53));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(54));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(55));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(56));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(57));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(58));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(59));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(60));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(61));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(62));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(63));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(64));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(65));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(66));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(67));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(68));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(69));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(70));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(71));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(72));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(73));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(74));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(75));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(76));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(77));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(78));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(79));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(80));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(81));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(82));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(83));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(84));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(85));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(86));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(87));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(88));
                #region 환자 타병원 전원
                GameObject restrictedArea2;
                GameObject sphere = Managers.Resource.Instantiate("System/ControlSphere", NPCs["보안요원2"].transform);
                {
                    NPCs["이송요원"].Teleport(WaitingArea);
                    NPCs["보안요원2"].Teleport(WaitingArea);

                    Managers.UI.ChangeChatBubble(NPCs["보안요원2"].transform, "신종감염병 의심환자 이송중으로 잠시 이동동선 통제가 있을 예정이니 환자 및 보호자분들의 양해 부탁드립니다.");

                    NPCs["보안요원2"].Teleport(Entrance);
                    NPCs["보안요원2"].AddOrder(NPCs["보안요원2"].CoGoDestination(BlockingPoint4));
                    NPCs["보안요원2"].AddOrder(NPCs["보안요원2"].CoSetForward(-Vector3.right));
                    NPCs["보안요원2"].AddOrder(NPCs["보안요원2"].CoSetState(CreatureState.Blocking));

                    yield return new WaitForSeconds(1.0f);

                    NPCs["이송요원"].Teleport(Entrance);
                    NPCs["이송요원"].AddOrder(NPCs["이송요원"].CoGoDestination(IsolationArea));

                    yield return new WaitUntil(() => (NPCs["보안요원2"].transform.position - BlockingPoint4).magnitude < 0.3f);
                    restrictedArea2 = Managers.Resource.Instantiate("System/IAToOutCollider");
                    yield return new WaitUntil(() => (NPCs["이송요원"].transform.position - IsolationArea).magnitude < 0.3f);

                    GameObject bed = GameObject.Find("move_bed");
                    bed.transform.SetParent(NPCs["이송요원"].transform);
                    NPCs["이송요원"].transform.GetChild(1).localPosition = new Vector3(0, 0, 1.2f);
                    NPCs["이송요원"].transform.GetChild(1).localEulerAngles = new Vector3(0, -90, 0);
                    NPCs["이송요원"].AddOrder(NPCs["이송요원"].CoGoDestination_Animation(ChangeBedPoint, CreatureState.Push));
                    NPCs["이송요원"].ChangeSpeed(3f);

                    yield return new WaitUntil(() => (NPCs["이송요원"].transform.position - ChangeBedPoint).magnitude < 0.3f);

                    //환자 이송이 끝나면 모든 NPC 상태 초기화, 출입구로 이동 후 퇴장 (환자 제외)
                    NPCs["이송요원"].ResetSpeed();

                    NPCs["이송요원"].AddOrder(NPCs["이송요원"].CoGoDestination(IAOutsideEntrancePoint));
                    NPCs["이송요원"].AddOrder(NPCs["이송요원"].CoTeleport(WaitingArea));
                }
                #endregion
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(89));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(90));
                yield return Managers.Instance.StartCoroutine(CoScenarioStep(91));
                #region 음압격리실 소독
                {
                    NPCs["미화1"].Teleport(Entrance);
                    NPCs["미화1"].AddOrder(NPCs["미화1"].CoGoDestination(IsolationArea));
                    NPCs["미화1"].AddOrder(NPCs["미화1"].CoUse("WetMop"));
                    yield return new WaitForSeconds(1.0f);

                    NPCs["미화2"].Teleport(Entrance);
                    NPCs["미화2"].AddOrder(NPCs["미화2"].CoGoDestination(BeforeIsolationArea));
                    NPCs["미화2"].AddOrder(NPCs["미화2"].CoUse("WetMop"));

                    yield return new WaitUntil(() => (NPCs["미화1"].Place == "음압격리실") && (NPCs["미화2"].Place == "전실"));
                    yield return new WaitForSeconds(5.0f);

                    NPCs["미화1"].AddOrder(NPCs["미화1"].CoGoDestination(IAOutsideEntrancePoint));
                    NPCs["미화2"].AddOrder(NPCs["미화2"].CoGoDestination(IAOutsideEntrancePoint));
                    NPCs["미화1"].AddOrder(NPCs["미화1"].CoTeleport(WaitingArea));
                    NPCs["미화2"].AddOrder(NPCs["미화2"].CoTeleport(WaitingArea));
                    NPCs["보안요원2"].AddOrder(NPCs["보안요원2"].CoGoDestination(IAOutsideEntrancePoint));
                    NPCs["보안요원2"].AddOrder(NPCs["보안요원2"].CoTeleport(WaitingArea));
                    Managers.Resource.Destroy(sphere);
;
                    yield return new WaitUntil(() => (NPCs["미화1"].Place == "대기장소") && (NPCs["미화2"].Place == "대기장소"));
                }
                Managers.Resource.Destroy(restrictedArea2);
                #endregion

                break;
        }

        #endregion

        UpdateScenarioAssist("시나리오를 완료하셨습니다.");
        Managers.UI.CreateSystemPopup("PopupNotice", $"{scenarioName} 시나리오를 완료하셨습니다.", UIManager.NoticeType.None);

        yield return new WaitForSeconds(3.0f);

        string position = Managers.Object.MyPlayer.Position;
        int score = Score <= 0 ? 0 : Score;

        Managers.Object.Clear();

        Managers.Scene.LoadSceneWait(Define.Scene.Lobby);
        Managers.Scene.AddWaitEvent(() =>
        {
            C_EndGame endPacket = new C_EndGame();
            endPacket.Position = position;
            endPacket.FinalScore = score;
            Managers.Network.Send(endPacket);
            Managers.Network.WaitingUI = Managers.UI.CreateUI("WaitingUI");
        });
    }

    #endregion

    #region 시나리오 진행 관련 기능

    //현재 진행된 시나리오에 대하여 다른 플레이어들이 확인할 수 있도록 말풍선, 메시지 등의 상황을 업데이트 해주는 함수
    void UpdateSituation()
    {
        switch (CurrentScenarioInfo.Action)
        {
            case "Call":
                Managers.Phone.Device.SendMessage(CurrentScenarioInfo.Position, CurrentScenarioInfo.OriginalSentence, CurrentScenarioInfo.Targets);
                break;
        }
    }

    //서버로부터 다음 시나리오 진행도를 받으면, 시나리오 상황 업데이트 및 변수 초기화 등 실행
    public void NextProgress(int progress)
    {
        if (CurrentScenarioInfo == null)
            return;

        if (progress == Progress)
            return;

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

        Util.FindChildByName(Hint, "HintSpeech").GetComponent<TMP_Text>().text = CurrentScenarioInfo.DetailHint;
    }

    public void UpdateMyPlace(string place = null)
    {
        string newPlace;

        if (place == null)
            newPlace = MyPlace.GetComponent<TMP_Text>().text;
        else
            newPlace = place;

        newPlace = Regex.Replace(newPlace, "<.*?>", string.Empty);
        _place = newPlace;

        //시나리오가 진행 중이고
        if (CurrentScenarioInfo != null)
        {
            //시나리오가 본인 차례일 경우
            if(CurrentScenarioInfo.Position == Managers.Object.MyPlayer.Position)
            {
                //시나리오 진행 위치가 null이 아니라면
                if (!string.IsNullOrEmpty(CurrentScenarioInfo.Place))
                {
                    //내 위치가 시나리오 진행 위치가 아니라면 붉은색으로 표시
                    if(CurrentScenarioInfo.Place != newPlace)
                    {
                        MyPlace.GetComponent<TMP_Text>().text = $"<color=#ff0000>{newPlace}</color>";
                        Navigation.gameObject.SetActive(true);
                        Navigation.SetDestination(CurrentScenarioInfo.Place);
                        return;
                    }
                }
            }
        }

        //그 외에는 검은색으로 표시
        MyPlace.GetComponent<TMP_Text>().text = $"<color=#000000>{newPlace}</color>";
        Navigation.gameObject.SetActive(false);
    }

    void UpdateObjectIndicator(List<string> objects)
    {
        foreach(var obj in ObjectIndicators)
        {
            Managers.Resource.Destroy(obj);
        }
        ObjectIndicators.Clear();

        if (CurrentScenarioInfo.Position != Managers.Object.MyPlayer.Position)
            return;

        if (objects.Count <= 0)
            return;

        foreach(string name in objects)
        {
            foreach(var obj in InteractableObjectsList)
            {
                if(obj.name == name)
                {
                    GameObject indicator = Managers.Resource.Instantiate(ObjectIndicator);
                    indicator.transform.parent = obj.transform;
                    ObjectIndicators.Add(indicator);
                }
            }
        }
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
        if (!string.IsNullOrEmpty(CurrentScenarioInfo.Place))
        {
            if (_place != CurrentScenarioInfo.Place)
            {
                Managers.UI.CreateSystemPopup("WarningPopup", "장소가 올바르지 않습니다.", UIManager.NoticeType.Warning);
                Reset();
                return false;
            }
        }

        if (MyAction != CurrentScenarioInfo.Action)
        {
            Managers.UI.CreateSystemPopup("WarningPopup", "올바른 행동을 수행하지 않았습니다.", UIManager.NoticeType.Warning);
            Reset();
            return false;
        }

        if (!CheckTarget())
        {
            Managers.UI.CreateSystemPopup("WarningPopup", "대상이 올바르지 않습니다.", UIManager.NoticeType.Warning);
            Reset();
            return false;
        }

        if (!string.IsNullOrEmpty(CurrentScenarioInfo.Item))
        {
            if (Item != CurrentScenarioInfo.Item)
            {
                Managers.UI.CreateSystemPopup("WarningPopup", "현재 상황에 알맞게 장비를 착용/해제 하지 않았습니다.", UIManager.NoticeType.Warning);
                Reset();
                return false;
            }
        }

        if (!(CurrentScenarioInfo.Action == "Tell" || CurrentScenarioInfo.Action == "Call"))
            return true;

        if (!PassSpeech)
        {
            Managers.UI.CreateSystemPopup("WarningPopup", "상황에 맞지 않는 대화이거나 대화 장소가 잘못되었습니다.", UIManager.NoticeType.Warning);
            Reset();
            return false;
        }

        Managers.UI.CreateSystemPopup("WarningPopup", "시나리오를 통과하셨습니다.", UIManager.NoticeType.Info);
        return true;
    }

    public bool CheckTarget()
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

        //시나리오 진행 장소가 정해져있는 경우, 현재 내가 해당 장소에 있는지 확인
        if (CurrentScenarioInfo.Place == Managers.Object.MyPlayer.Place)
            return true;
        else
        {
            Managers.UI.CreateSystemPopup("WarningPopup", $"{CurrentScenarioInfo.Place}에서 시나리오를 수행하세요.", UIManager.NoticeType.Warning);
            return false;
        }
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
                message = message.Replace(keyword, $"<color=#0000ff>{keyword}</color>");
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
        Managers.Object.Characters.Add(position, nc);

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

    #region PlayerNPC 제어 기능

    bool AddPlayerNPC(string position)
    {
        GameObject go = Managers.Resource.Instantiate($"Creatures/PlayerNPC/{position}");

        if (go == null)
        {
            Debug.LogError($"Can't find {position} PlayerNPC prefab");
            return false;
        }

        PlayerNPCController nc = go.GetComponent<PlayerNPCController>();
        nc.Position = position;
        PlayerNPCs.Add(nc.Position, nc);
        Managers.Object.Characters.Add(position, nc);
        nc.GenerateNumber = PlayerNPCs.Count - 1;
        nc.WaitingPoint = new Vector3(PlayerNPCSpawnPoint.x, PlayerNPCSpawnPoint.y, PlayerNPCSpawnPoint.z + -5 * nc.GenerateNumber);

        nc.Teleport(nc.WaitingPoint);

        return true;
    }
    public void CheckPlayerNPCComplete()
    {
        if (PlayerNPCs.Count <= 0)
            AllPlayerNPCCompleted = true;

        foreach (var npc in PlayerNPCs.Values)
        {
            if (npc.PassScenario == false)
                AllPlayerNPCCompleted = false;
        }

        AllPlayerNPCCompleted = true;
    }

    public IEnumerator CoInitPlayerNPCs()
    {
        if (PlayerNPCs.Count <= 0)
            yield break;

        Dictionary<PlayerNPCController, Coroutine> coDict = new Dictionary<PlayerNPCController, Coroutine>();

        foreach (var npc in PlayerNPCs.Values)
        {
            coDict.Add(npc, npc.StartCoroutine(npc.AutoActionScenario(CurrentScenarioInfo)));
        }

        foreach (var npc in PlayerNPCs.Values)
        {
            if(npc.PassScenario == false)
            {
                yield return coDict[npc];
            }
        }

        CheckPlayerNPCComplete();
        yield break;
    }

    #endregion

    bool ShouldShowYudoLine(int progress)
    {
        int[] showYudoLineProgress = { 24, 29, 46, 66 };

        return Array.Exists(showYudoLineProgress, p => p == progress);
    }

    public void Clear()
    {
        TTSPlaying = false;
        CompleteCount = 0;
        ScenarioName = null;
        Progress = 0;
        Item = null;
        Score = 100;
        NPCs.Clear();
        _realtimeSTT = null;
        _scenarioAssist = null;
        _scenarioHint = false;
        _checkComplete = false;
        Reset();
        _doingScenario = false;
        _routine = null;
        PopupConfirm = 0;
        CurrentScenarioInfo = null;
        InteractableObjectsList.Clear();
        ObjectIndicators.Clear();
        Define.WaitingCount = 0;
        Navigation = null;
        PlayerNPCs.Clear();
        if(_scenarioCoroutine != null)
        {
            Managers.Instance.StopCoroutine(_scenarioCoroutine);
            _scenarioCoroutine = null;
        }
    }
}
