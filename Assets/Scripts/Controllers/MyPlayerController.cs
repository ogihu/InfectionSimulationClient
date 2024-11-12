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

    [SerializeField] private float _syncTimer = 0.2f;
    [SerializeField] float _camRotationSpeed;
    [SerializeField] float raycastDistance = 2f;
    public Arrow_Deactivate Arrow_Deactivate;
    public CameraArm _cameraArm;
    float mouseX = 0f;
    Coroutine _coSendPacket;
    public GameObject _interactionObject;
    int _layerMask;
    public override void Awake()
    {
        base.Awake();
        GameObject cameraArm = Managers.Resource.Instantiate("System/CameraArm", this.gameObject.transform);
         GameObject Arrow = Managers.Resource.Instantiate($"Items/Arrow_Deactivate", this.gameObject.transform);
        _cameraArm = cameraArm.GetComponent<CameraArm>();
        Arrow_Deactivate = Arrow.GetComponent<Arrow_Deactivate>();
        _coSendPacket = StartCoroutine(CoSyncUpdate());
        _layerMask = 1 << LayerMask.NameToLayer("Interaction");
        Managers.Setting.SceneStartMicCheck();
       
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
        if (!IsCanActive() || Managers.Item.IsInventoryOpen)
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

        if (Managers.EMR.DoingEMR)
            return false;

        if (State == CreatureState.Idle || State == CreatureState.Run)
            return true;

        return false;
    }

    void GetKeyInput()
    {
        if (IsCanActive())
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
                            Managers.Scenario.MyAction = "SCRFWrite";
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
            Managers.Item.OpenOrCloseInventory();
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            if (!Managers.STT.MySpeech.activeSelf)
                return;
            else
                Managers.STT.ChangeSpeechState();
        }

        //팝업 닫기 or 설정 열기/닫기
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!Managers.UI.ExitPopup())
            {
                if (State == CreatureState.Idle || State == CreatureState.Setting)
                    Managers.UI.OpenOrCloseSetting();
            }
        }

        //휴대폰 사용/종료
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (State == CreatureState.Idle)
            {
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
            if (!Managers.Scenario._doingScenario)
                return;

            if (Managers.Scenario.CurrentScenarioInfo != null)
            {
                if (Managers.Scenario.CurrentScenarioInfo.Action != "Tell")
                {
                    Managers.UI.CreateSystemPopup("WarningPopup", "현재 사용할 수 없는 기능입니다.", UIManager.NoticeType.None);
                    return;
                }
            }

            if (!Managers.Scenario.CheckPlace())
                return;

            if (State == CreatureState.Idle)
            {
                Managers.Scenario.MyAction = "Tell";
                if (Managers.Setting.UsingMic)
                {
                    Managers.STT.GoogleSpeechObj.GetComponent<StreamingRecognizer>().StartListening();
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
                    Managers.STT.GoogleSpeechObj.GetComponent<StreamingRecognizer>().StopListening();
                    Managers.STT.GoogleSpeechObj.GetComponent<StreamingRecognizer>().TextUI.GetComponent<AccumulateText>().FinalEvaluate();
                }
                else
                {
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
            yield return new WaitForSeconds(_syncTimer);
        }
    }
}