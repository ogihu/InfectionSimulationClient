using Google.Protobuf.Protocol;
using GoogleCloudStreamingSpeechToText;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;
using static Define;
using static Google.Cloud.Speech.V1.LanguageCodes;


public class MyPlayerController : PlayerController
{
    public override CreatureState State
    {
        get
        {
            return base.State;
        }
        set
        {
            base.State = value;
            UpdateCursor();
        }
    }

    [SerializeField] private float _syncTimer = 0.1f;
    [SerializeField] float _camRotationSpeed;
    [SerializeField] float raycastDistance = 2f;
    public Navigation Navigation;
    public CameraArm _cameraArm;
    float mouseX = 0f;
    Coroutine _coSendPacket;
    public GameObject _interactionObject;
    int _layerMask;
    WaitForSeconds _waitSyncTimer;

    public override void Awake()
    {
        base.Awake();
        GameObject cameraArm = Managers.Resource.Instantiate("System/CameraArm", this.gameObject.transform);
        GameObject navigation = Managers.Resource.Instantiate($"Items/Navigation", this.gameObject.transform);
        _cameraArm = cameraArm.GetComponent<CameraArm>();
        Navigation = navigation.GetComponent<Navigation>();
        _coSendPacket = StartCoroutine(CoSyncUpdate());
        _layerMask = 1 << LayerMask.NameToLayer("Interaction");
        Managers.Setting.SceneStartMicCheck();
        _waitSyncTimer = new WaitForSeconds(_syncTimer);
    }

    protected override void UpdateController()
    {
        base.UpdateController();
        GetKeyInput();
        UpdateObjectRay();
        UpdateWorldUIRay();
    }

    protected override void UpdateMove()
    {
        base.UpdateMove();

        if (State == CreatureState.Run)
            Pos = transform.position;
    }

    protected override void UpdateRotation()
    {
        if (!IsCanActive() || Managers.Item.IsInventoryOpen || !Managers.Scenario.PassUICheck)
            return;

        Quaternion currentRotation = this.transform.localRotation;

        _cameraArm.CameraRotation(_camRotationSpeed);
        mouseX += Input.GetAxis("Mouse X") * _camRotationSpeed;
        Quaternion targetRotation = Quaternion.Euler(0, mouseX, 0);

        this.transform.localRotation = Quaternion.Slerp(currentRotation, targetRotation, Time.deltaTime * _camRotationSpeed * 5);

        Dir = new Vector3(Camera.main.transform.forward.x, 0, Camera.main.transform.forward.z);
    }

    void UpdateCursor()
    {
        Cursor.lockState = (!(IsCanActive() || State == CreatureState.PickUp) || Managers.Item.IsInventoryOpen) ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = (!(IsCanActive() || State == CreatureState.PickUp) || Managers.Item.IsInventoryOpen);
    }

    public bool IsCanActive()
    {
        if (Managers.Quiz.QuizUI != null)
            return false;

        if(Managers.Scenario.State_Image)
            return false;

        if (Managers.Item.IsInventoryOpen)
            return false;

        if (Managers.Quiz.MPX_Clothing_Panel_opencheck)
            return false;

        if (Managers.EMR.DoingEMR)
            return false;

        if (State == CreatureState.Idle || State == CreatureState.Run)
            return true;

        return false;
    }

    void GetKeyInput()
    {
        if (IsCanActive() && Managers.Scenario.PassUICheck)
        {
            int lastValue = InputBit;
            InputBit = Managers.Input.SetKeyInput(KeyCode.W, InputBit);
            InputBit = Managers.Input.SetKeyInput(KeyCode.A, InputBit);
            InputBit = Managers.Input.SetKeyInput(KeyCode.S, InputBit);
            InputBit = Managers.Input.SetKeyInput(KeyCode.D, InputBit);

            if (lastValue != InputBit)
                CheckUpdatedFlag();
        }
        else
            InputBit = 0;

        #region 기능 키

        //아이템 얻기
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (_interactionObject != null)
            {
                if (_interactionObject.name == "EMRPC")
                {
                    if(Managers.Scenario.CurrentScenarioInfo != null)
                    {
                        if(Managers.Scenario.CurrentScenarioInfo.Action == "EMRWrite")
                        {
                            Managers.EMR.OpenEMRWrite();
                        }
                        else  if (Managers.Scenario.CurrentScenarioInfo.Action == "EMRRead")
                        {
                            Managers.EMR.OpenEMRRead();
                        }
                        else if(Managers.Scenario.CurrentScenarioInfo.Action == "SCRFWrite")
                        {
                            Managers.EMR.OpenForm();
                        }
                        return;
                    }
                    Managers.UI.CreateSystemPopup("WarningPopup", "현재 사용할 수 없는 기능입니다.", UIManager.NoticeType.None);
                }
                else if (_interactionObject.name == "X-Ray")
                {
                    Managers.Scenario.MyAction = "X-Ray";
                }
                else if (State == CreatureState.Idle)
                {
                    GetItem();  // 아이템 상호작용
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            Managers.Scenario.ScenarioAssist_HintActive();
        }

        //인벤토리 열기/닫기
        if (Input.GetKeyDown(KeyCode.I))
        {
            if (State == CreatureState.UsingPhone)
                return;

            if (Managers.Scenario.PassUICheck == false)
                return;

            if (Items.ContainsKey("Syringe") || Items.ContainsKey("DrySwab"))
            {
                return;
            }

            Managers.Item.OpenOrCloseInventory();
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            if (!Managers.STT.MySpeech.activeSelf)
                return;
            else
                Managers.STT.ChangeSpeechState();
            //Managers.STT.ChangeSpeechState();
        }

        //팝업 닫기 or 설정 열기/닫기
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!Managers.UI.ExitPopup())
            {
                if (State == CreatureState.Idle || State == CreatureState.Setting)
                {
                    if (Managers.Scenario.PassUICheck == false)
                        return;

                    Managers.UI.OpenOrCloseSetting();
                }
            }
        }

        //휴대폰 사용/종료
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (State == CreatureState.Idle)
            {
                if (Managers.Item.IsInventoryOpen)
                {
                    return;
                }


                if (Managers.Scenario.PassUICheck == false)
                    return;

                if (Items.ContainsKey("Syringe") || Items.ContainsKey("DrySwab"))
                {
                    return;
                }

                Managers.Phone.OpenPhone();
            }
            else if (State == CreatureState.UsingPhone)
            {
                if (Managers.Phone.Device._isCalling)
                    Managers.Phone.Device.FinishCall();
                else
                    Managers.Phone.ClosePhone();
            }
        }

        //대화하기 or 키워드
        if (Input.GetKeyDown(KeyCode.T))
        {
            #region 시나리오 시작 전

            if (!Managers.Scenario._doingScenario)  
            {
                Managers.Scenario.SendScenarioInfo();
                return;
            }

            if (Managers.Scenario.CurrentScenarioInfo == null)
            {
                Managers.UI.CreateSystemPopup("WarningPopup", "시나리오가 시작되지 않았습니다.", UIManager.NoticeType.None);
                return;
            }

            if (Managers.Scenario.CurrentScenarioInfo != null)
            {
                if(Managers.Scenario.CurrentScenarioInfo.Position != Position)
                {
                    Managers.UI.CreateSystemPopup("WarningPopup", "현재 사용할 수 없는 기능입니다.", UIManager.NoticeType.None);
                    return;
                }
               
                if (Managers.Scenario.CurrentScenarioInfo.Action != "Tell" && (Managers.Scenario.CurrentScenarioInfo.Action != "MPX_Clothing" && Managers.Scenario.CurrentScenarioInfo.Action != "MPX_LayOff"))
                {
                    Managers.UI.CreateSystemPopup("WarningPopup", "현재 사용할 수 없는 기능입니다.", UIManager.NoticeType.None);
                    return;
                }
            }

            #endregion
            
            if (Managers.Scenario.CurrentScenarioInfo.Action == "MPX_Clothing" || Managers.Scenario.CurrentScenarioInfo.Action == "MPX_LayOff")
            {
                if ((Managers.Object.MyPlayer.Place == Managers.Scenario.CurrentScenarioInfo.Place )||(Managers.Scenario.CurrentScenarioInfo.Place == null))
                {
                    if (Managers.Quiz.MPX_Clothing_Panel == null)
                    {
                        Managers.Quiz.MPX_Clothing_Panel = Managers.UI.CreateUI("MPX_Clothing_Panel");
                        return;
                    }

                    else if (Managers.Quiz.MPX_Clothing_Panel.GetComponent<MPX_Clothing_Panel>().child != null)
                        return;

                    if (Managers.Quiz.MPX_Clothing_Panel.GetComponent<MPX_Clothing_Panel>().child == null)
                        Managers.Quiz.MPX_Clothing_Panel.GetComponent<MPX_Clothing_Panel>().Open_MPX_Panel();

                    return;
                }
            }

            if (!Managers.Scenario.CheckPlace())
                return;

            if (State == CreatureState.Idle)
            {
                if (Managers.Object.Characters.ContainsKey(Managers.Scenario.CurrentScenarioInfo.Targets[0]))
                {
                    List<BaseController> nearChar = new List<BaseController>();
                    LayerMask ignoreLayers = (1 << LayerMask.NameToLayer("Player")) | (1 << LayerMask.NameToLayer("NPC"));
                    nearChar = Util.FindComponentInRange<BaseController>(gameObject, 7, ignoreLayers);

                    if (nearChar == null)
                    {
                        Managers.UI.CreateSystemPopup("WarningPopup", "대화 대상과의 거리가 너무 멉니다.", UIManager.NoticeType.None);
                        return;
                    }

                    bool isNear = false;

                    foreach (var character in nearChar)
                    {
                        if (Managers.Scenario.CurrentScenarioInfo.Targets.Contains(character.Position))
                        {
                            isNear = true;
                            break;
                        }
                    }

                    if (!isNear)
                    {
                        Managers.UI.CreateSystemPopup("WarningPopup", "대화 대상과의 거리가 너무 멉니다.", UIManager.NoticeType.None);
                        return;
                    }
                }

                Managers.Scenario.MyAction = "Tell";
                Managers.Scenario.Targets.Add(Managers.Scenario.CurrentScenarioInfo.Targets[0]);

                if (Managers.Setting.UsingMic)
                {
                    Managers.STT.GoogleSpeechObj.GetComponent<CustomStreamingRecognizer>().StartListening();
                }
                else
                {
                    Managers.Keyword.OpenGUIKeyword();
                }
                
            }
            else if (State == CreatureState.Conversation)
            {
                if (Managers.Setting.UsingMic)
                {
                    Managers.STT.GoogleSpeechObj.GetComponent<CustomStreamingRecognizer>().StopListening();
                    Managers.STT.GoogleSpeechObj.GetComponent<CustomStreamingRecognizer>().TextUI.GetComponent<AccumulateText>().FinalEvaluate();
                }
                else
                {
                    if(Managers.Keyword.CanClose)
                        Managers.Keyword.CloseGUIKeyword();
                }
            }
        }

        //시나리오 스킵
        if (Input.GetKeyDown(KeyCode.Home))
        {
            Managers.Scenario.CompleteCount++;
        }

        //말풍선 이전 문장
        if (Input.GetKeyDown(KeyCode.Z))
        {
            Managers.Bubble.PrevPage();
        }

        //말풍선 다음 문장
        if (Input.GetKeyDown(KeyCode.X))
        {
            Managers.Bubble.NextPage();
        }

        //말풍선 닫기/열기
        if (Input.GetKeyDown(KeyCode.C))
        {
            Managers.Bubble.OpenOrCloseBubble();
        }

        #endregion
    }

    void UpdateObjectRay()
    {
        if (!IsCanActive())
        {
            if (_interactionObject != null)
                _interactionObject.GetComponent<InteractableObject>().InActiveKeyUI();

            _interactionObject = null;
            return;
        }

        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);

        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo, raycastDistance, _layerMask))
        {
            if (hitInfo.transform.gameObject == _interactionObject)
                return;

            if (_interactionObject != null)
            {
                if (hitInfo.transform.gameObject != _interactionObject)
                {
                    _interactionObject.GetComponent<InteractableObject>().InActiveKeyUI();
                }
            }

            _interactionObject = hitInfo.transform.gameObject;
            _interactionObject.GetComponent<InteractableObject>().ActiveKeyUI();

        }
        else
        {
            if (_interactionObject != null)
            {
                _interactionObject.GetComponent<InteractableObject>().InActiveKeyUI();
                _interactionObject = null;
            }
        }

#if UNITY_EDITOR
        Debug.DrawRay(ray.origin, ray.direction * 100, Color.red);
#endif
    }

    void UpdateWorldUIRay()
    {
        if (!IsCanActive())
            return;

        if (Input.GetMouseButtonDown(0))
        {
            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            if (results.Count > 0)
            {
                RaycastResult closestResult = results
                .Where(result => result.gameObject.layer == LayerMask.NameToLayer("Bubble"))
                .OrderBy(result => result.distance)
                .FirstOrDefault();

                if (closestResult.gameObject != null)
                {
                    Button button = closestResult.gameObject.GetComponent<Button>();

                    if (button != null)
                    {
                        button.onClick.Invoke();
                        if (Managers.Bubble.SelectedChat != null)
                        {
                            Managers.Bubble.ChangeButtonColor();
                        }
                    }
                }
            }
        }
    }

    void GetItem()
    {
        if (_interactionObject == null)
            return;

        InteractableObject obj = _interactionObject.GetComponent<InteractableObject>();

        if (obj == null)
        {
            Debug.Log("This object don't have component : InteractableObject");
            return;
        }
        if (_interactionObject.name == "Computer")
        {
            State = CreatureState.Sit;
        }
        obj.GetItem();
        State = CreatureState.PickUp;
        StartCoroutine(CoDelayIdle(0.95f));
    }

    IEnumerator CoDelayIdle(float time)
    {
        yield return new WaitForSeconds(time);
        State = CreatureState.Idle;
    }

    private void SendSyncPacket()
    {
        if (_syncUpdated)
        {
            if (transform.position.y < -5)
            {
                transform.position = Vector3.zero;
                PosInfo.PosX = transform.position.x;
                PosInfo.PosY = transform.position.y;
                PosInfo.PosZ = transform.position.z;
            }

            C_Sync syncPacket = new C_Sync();
            syncPacket.PosInfo = PosInfo;
            Managers.Network.Send(syncPacket);
            _syncUpdated = false;
        }
    }

    private void SendMovePacket()
    {
        if (_updated)
        {
            C_Move movePacket = new C_Move();
            movePacket.MoveInfo = MoveInfo;
            Managers.Network.Send(movePacket);
            _updated = false;
        }
    }
    
    private void CheckUpdatedFlag()
    {
        SendSyncPacket();
        SendMovePacket();
    }

    IEnumerator CoSyncUpdate()
    {
        while (true)
        {
            CheckUpdatedFlag();
            yield return _waitSyncTimer;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red; // Gizmo의 색상을 설정합니다.
        Gizmos.DrawWireSphere(transform.position, 6); // 감지 반경을 그립니다.
    }
#endif
}