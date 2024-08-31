using Google.Protobuf.Protocol;
using POpusCodec;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;
using static Define;

public class BaseController : MonoBehaviour
{
    #region Fields

    public int ObjectId { get; set; }

    #region MoveInfo MoveInfo

    MoveInfo _moveInfo = new MoveInfo();
    public MoveInfo MoveInfo
    {
        get { return _moveInfo; }
        set
        {
            if (_moveInfo.Equals(value))
                return;

            State = value.State;
            Dir = new Vector3(value.DirX, 0, value.DirZ);
            InputBit = value.InputBit;
        }
    }

    //캐릭터 상태
    public virtual CreatureState State
    {
        get { return MoveInfo.State; }
        set
        {
            if (MoveInfo.State == value)
                return;

            MoveInfo.State = value;
            _updated = true;
        }
    }

    public Vector3 Dir
    {
        get { return new Vector3(MoveInfo.DirX, 0, MoveInfo.DirZ); }
        set
        {
            if (MoveInfo.DirX == value.x && MoveInfo.DirZ == value.z)
                return;

            MoveInfo.DirX = value.x;
            MoveInfo.DirZ = value.z;
            _updated = true;
        }
    }

    /// <summary>
    /// W - 0, A - 1, S - 2, D - 3
    /// </summary>
    public int InputBit
    {
        get { return MoveInfo.InputBit; }
        set
        {
            if (MoveInfo.InputBit == value)
                return;

            MoveInfo.InputBit = value;
            _updated = true;
        }
    }

    #endregion

    #region PosInfo PosInfo

    PosInfo _posInfo = new PosInfo();
    public PosInfo PosInfo
    {
        get { return _posInfo; }
        set
        {
            if (_posInfo.Equals(value))
                return;

            Pos = new Vector3(value.PosX, value.PosY, value.PosZ);
        }
    }

    public Vector3 Pos
    {
        get { return new Vector3(PosInfo.PosX, PosInfo.PosY, PosInfo.PosZ); }
        set
        {
            if (PosInfo.PosX == value.x && PosInfo.PosY == value.y && PosInfo.PosZ == value.z)
                return;

            PosInfo.PosX = value.x;
            PosInfo.PosY = value.y;
            PosInfo.PosZ = value.z;
            _syncUpdated = true;
        }
    }

    #endregion

    #region UserInfo UserInfo

    UserInfo _userInfo = new UserInfo();
    public UserInfo UserInfo
    {
        get { return _userInfo; }
        set
        {
            if (_userInfo.Equals(value))
                return;

            Name = value.Name;
            Id = value.Id;
            Position = value.Position;
        }
    }

    public string Name
    {
        get { return UserInfo.Name; }
        set
        {
            if (UserInfo.Name.Equals(value))
                return;

            UserInfo.Name = value;
        }
    }

    public string Id
    {
        get { return UserInfo.Id; }
        set
        {
            if (UserInfo.Id.Equals(value))
                return;

            UserInfo.Id = value;
        }
    }

    public string Position
    {
        get { return UserInfo.Position; }
        set
        {
            if (UserInfo.Position.Equals(value))
                return;

            UserInfo.Position = value;
        }
    }

    #endregion

    protected List<Animator> _equipmentAnimator = new List<Animator>();
    protected Animator _animator;
    string _currentAnimation;

    protected bool _syncUpdated = false;
    protected bool _updated = false;
    [SerializeField] protected float _speed = 1.5f;
    protected Vector3 _destPos = Vector3.zero;
    [Tooltip("MyPlayer는 적용되지 않음")]
    [SerializeField] protected float _rotationSpeed = 8f;
    public string Place { get; set; }
    protected GameObject _usingItem;
    Coroutine _coSync;
    public GameObject _positionDisplay;
    public Dictionary<string, GameObject> Equipment = new Dictionary<string, GameObject>();

    protected AudioSource _audioSource;

    //도구 부착할 Transform
    Transform _rightHand;
    Transform _leftHand;

    #endregion

    public virtual void CopyTo(BaseController target)
    {
        target.ObjectId = this.ObjectId;
        target.Pos = this.Pos;
        target.Dir = this.Dir;
        target.State = this.State;
        target.InputBit = this.InputBit;
        target.Name = this.Name;
        target.Position = this.Position;
        target.Id = this.Id;
        target.Equipment = this.Equipment;
    }

    public virtual void Awake()
    {
        _animator = GetComponent<Animator>();

        _audioSource = GetComponent<AudioSource>();
        //VoiceBuffer = new VoiceBuffer<float>(VoiceFrequency * VoiceChannel * 10);

        _leftHand = Util.FindChildByName(this.gameObject, "L_hand_grap_point").transform;
        _rightHand = Util.FindChildByName(this.gameObject, "R_hand_grap_point").transform;

        Managers.UI.CreateChatBubble(this.transform);
        //_voiceClip = AudioClip.Create("VoiceClip", VoiceFrequency * VoiceChannel, VoiceChannel, VoiceFrequency, true, OnAudioRead);
        //_decoder = new OpusDecoder(POpusCodec.Enums.SamplingRate.Sampling16000,
        //                                  POpusCodec.Enums.Channels.Stereo);
    }

    public virtual void Start()
    {
        if (Position != null)
        {
            _positionDisplay = Managers.UI.CreateUI("PositionDisplay", UIManager.CanvasType.World);
            _positionDisplay.GetComponent<FloatingUI>().Init(transform, y : 2.0f);
            _positionDisplay.GetComponent<FloatingUI>().ChangeMessage(Position);
            Debug.Log($"{Position} 캐릭터 생성");
        }
    }

    private void Update()
    {
        UpdateController();
    }

    private void OnDestroy()
    {
        if(_positionDisplay != null)
        {
            Destroy(_positionDisplay);
        }
    }

    protected virtual void UpdateController()
    {
        UpdateMove();
        UpdateRotation();
        UpdateAnimation();
    }

    protected virtual void UpdateMove()
    {
        if (State != CreatureState.Idle && State != CreatureState.Run)
            return;

        if (InputBit == 0 && State != CreatureState.Idle)
        {
            if (State != CreatureState.Idle)
                State = CreatureState.Idle;

            return;
        }

        Vector3 moveDir = Vector3.zero;

        if (Managers.Input.GetKeyInput(KeyCode.W, InputBit))
        {
            moveDir += Vector3.forward;
        }
        if (Managers.Input.GetKeyInput(KeyCode.A, InputBit))
        {
            moveDir += Vector3.left;
        }
        if (Managers.Input.GetKeyInput(KeyCode.S, InputBit))
        {
            moveDir += Vector3.back;
        }
        if (Managers.Input.GetKeyInput(KeyCode.D, InputBit))
        {
            moveDir += Vector3.right;
        }

        if (moveDir == Vector3.zero)
        {
            State = CreatureState.Idle;
            return;
        }

        State = CreatureState.Run;
        moveDir = moveDir.normalized;
        transform.Translate(moveDir * _speed * Time.deltaTime);
    }

    protected virtual void UpdateRotation()
    {
        if (Dir == Vector3.zero)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(Dir);

        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * _rotationSpeed);
    }

    protected virtual void UpdateAnimation()
    {
        string newAnimation = GetAnimationByState(State);
        if(_currentAnimation != newAnimation)
        {
            _animator.Play(newAnimation);

            if (_equipmentAnimator.Count > 0) 
            {
                foreach (var animator in _equipmentAnimator)
                    animator.Play(newAnimation);
            }

            _currentAnimation = newAnimation;
        }
        ActiveObjectOnState();
    }

    private string GetAnimationByState(CreatureState state)
    {
        switch (state)
        {
            case CreatureState.Idle:
            case CreatureState.Setting:
            case CreatureState.UsingInventory:
                return "idle";

            case CreatureState.Run:
                return "run";

            case CreatureState.Conversation:
                return "conversation";

            case CreatureState.UsingPhone:
                return "using-phone";

            case CreatureState.Clean:
                return "clean";

            case CreatureState.PickUp:
                return "pickup";

            case CreatureState.Push:
                return "push";

            case CreatureState.Sit:
                return "sit";

            case CreatureState.Typing:
                return "typing";

            default:
                return "idle";
        }
    }

    void ActiveObjectOnState()
    {
        if (State == CreatureState.UsingPhone)
        {
            if (_usingItem == null)
                _usingItem = Managers.Resource.Instantiate("Objects/Phone", _leftHand);
        }
        else if (State == CreatureState.Clean)
        {
            if (_usingItem == null)
                _usingItem = Managers.Resource.Instantiate("Objects/Spray", _rightHand);
        }
        else
        {
            if (_usingItem != null)
                Managers.Resource.Destroy(_usingItem);
        }
    }

    public bool EquipItem(GameObject equipment)
    {
        if (Equipment.ContainsKey(equipment.name))
            return false;

        Equipment.Add(equipment.name, equipment);
        
        if (equipment.GetComponent<Animator>() != null)
            _equipmentAnimator.Add(equipment.GetComponent<Animator>());

        return true;
    }

    public void UnEquipItem(string equipment)
    {
        if (Equipment.ContainsKey(equipment))
        {
            GameObject go = Equipment[equipment];
            Equipment.Remove(equipment);

            Animator animator = go.GetComponent<Animator>();
            if(animator != null)
            {
                if(_equipmentAnimator.Contains(animator))
                    _equipmentAnimator.Remove(animator);
            }

            Managers.Resource.Destroy(go);
        }
    }

    public void UpdateSync()
    {
        float distance = (Pos - transform.position).magnitude;

        if (distance > 2.0f)
            ImmediateSync();
    }

    public void ImmediateSync()
    {
        if (transform.position != Pos)
            transform.position = Pos;
    }

    void MakeSound(string clipName)
    {
        Managers.Sound.Play(_audioSource, clipName);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("Map"))
            return;

        Place = other.gameObject.name;
    }
}
