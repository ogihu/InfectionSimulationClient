using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
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

    protected Animator _animator;
    protected bool _syncUpdated = false;
    protected bool _updated = false;
    [SerializeField] private float _speed = 1.5f;
    protected Vector3 _destPos = Vector3.zero;
    [Tooltip("MyPlayer는 적용되지 않음")]
    [SerializeField] protected float _rotationSpeed = 8f;
    public string Place { get; set; }
    protected GameObject _usingItem;
    Coroutine _coSync;
    GameObject _positionDisplay;
    public GameObject Equipment;

    protected AudioSource _audioSource;
    public VoiceBuffer<float> VoiceBuffer { get; set; }
    //AudioClip _voiceClip;
    //int _clipSamplePosition = 0;
    //bool _isTalking = false;

    //도구 부착할 Transform
    Transform _rightHand;
    Transform _leftHand;

    #endregion

    public virtual void Awake()
    {
        _animator = GetComponent<Animator>();

        _audioSource = GetComponent<AudioSource>();
        VoiceBuffer = new VoiceBuffer<float>(VoiceFrequency * VoiceChannel * 5);
        //_voiceClip = AudioClip.Create("VoiceClip", VoiceFrequency * VoiceChannel * 10, VoiceChannel, VoiceFrequency, false);
        //_audioSource.clip = _voiceClip;

        _leftHand = Util.FindChildByName(this.gameObject, "L_hand_grap_point").transform;
        _rightHand = Util.FindChildByName(this.gameObject, "R_hand_grap_point").transform;

        Managers.UI.CreateChatBubble(this.transform);
    }

    public virtual void Start()
    {
        if (Position != null)
        {
            _positionDisplay = Managers.UI.CreateUI("PositionDisplay", UIManager.CanvasType.World);
            _positionDisplay.GetComponent<FloatingUI>().Init(transform, 2.0f);
            _positionDisplay.GetComponent<FloatingUI>().ChangeMessage(Position);
        }
        //LoadMeshAndMat("ProtectedGear");
    }

    private void Update()
    {
        UpdateController();
    }

    protected virtual void UpdateController()
    {
        UpdateMove();
        UpdateRotation();
        UpdateAnimation();
    }

    protected virtual void UpdateMove()
    {
        if (!(State == CreatureState.Idle || State == CreatureState.Run))
            return;

        if (InputBit == 0)
        {
            if(State != CreatureState.Idle)
                State = CreatureState.Idle;

            return;
        }

        Vector3 moveDir = Vector3.zero;

        if(Managers.Input.GetKeyInput(KeyCode.W, InputBit))
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
        switch (State)
        {
            case CreatureState.Idle:
            case CreatureState.Setting:
            case CreatureState.UsingInventory:
                _animator.Play("idle");
                break;
            case CreatureState.Run:
                _animator.Play("run");
                break;
            case CreatureState.Conversation:
                _animator.Play("conversation");
                break;
            case CreatureState.UsingPhone:
                _animator.Play("using-phone");
                break;
            case CreatureState.Clean:
                _animator.Play("clean");
                break;
            case CreatureState.PickUp:
                _animator.Play("pickup");
                break;
            default:
                Debug.LogError($"There is no animation clip about {State}");
                break;
        }

        ActiveObjectOnState();
    }

    void ActiveObjectOnState()
    {
        if(State == CreatureState.UsingPhone)
        {
            if(_usingItem == null)
                _usingItem = Managers.Resource.Instantiate("Objects/Phone", _leftHand);
        }
        else if (State == CreatureState.Clean)
        {
            if (_usingItem == null)
                _usingItem = Managers.Resource.Instantiate("Objects/Spray", _rightHand);
        }
        else
        {
            if(_usingItem != null)
                Managers.Resource.Destroy(_usingItem);
        }
    }

    public void AddEquipment(GameObject equipment)
    {
        RemoveEquipment();
        Equipment = equipment;
    }

    public void RemoveEquipment()
    {
        if (Equipment != null)
            Equipment.GetComponent<Equipment>().UnEquip(this);
    }

    public void UpdateSync()
    {
        float distance = (Pos - transform.position).magnitude;

        if (distance > 2.0f)
            ImmediateSync();
    }

    public void ImmediateSync()
    {
        if(transform.position != Pos)
            transform.position = Pos;
    }

    public void LoadMeshAndMat(string name)
    {
        SkinnedMeshRenderer smr = GetComponent<SkinnedMeshRenderer>();
        smr.sharedMesh = Resources.Load<Mesh>($"Models/Characters/{name}");
        Material[] materials = Resources.LoadAll<Material>($"Materials/Characters/{name}");
        smr.materials = materials;
    }

    public virtual void ChangeCharacter(string name)
    {
        GameObject cloth = Managers.Resource.Instantiate("Creatures/Models/ProtectedGear", gameObject.transform);
        SkinnedMeshRenderer characterRenderer = GetComponent<SkinnedMeshRenderer>();
        SkinnedMeshRenderer clothRenderer = cloth.GetComponent<SkinnedMeshRenderer>();

        clothRenderer.bones = characterRenderer.bones;
        clothRenderer.rootBone = characterRenderer.rootBone;
    }

    void MakeSound(string clipName)
    {
        Managers.Sound.Play(_audioSource, clipName);
    }

    public void ReceiveVoiceBuffer(float[] samples)
    {
        float[] stereoSamples = ConvertMonoToStereo(samples);
        VoiceBuffer.Write(stereoSamples);
        ReadVoiceBuffer();
    }

    private void ReadVoiceBuffer()
    {
        if (VoiceBuffer.Count < VoiceFrequency * VoiceChannel)
            return;

        float[] playSamples = new float[VoiceFrequency * VoiceChannel];
        VoiceBuffer.Read(playSamples, VoiceFrequency * VoiceChannel);
        AudioClip voiceClip = AudioClip.Create("VoiceClip", VoiceFrequency * VoiceChannel, VoiceChannel, VoiceFrequency, false);
        voiceClip.SetData(playSamples, 0);
        _audioSource.PlayOneShot(voiceClip);
        //_voiceClip.SetData(playSamples, _clipSamplePosition);
        //_clipSamplePosition = (_clipSamplePosition + VoiceFrequency * VoiceChannel) % _voiceClip.samples;

        //if (!_audioSource.isPlaying)
        //    _audioSource.Play();
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("Map"))
            return;

        Place = other.gameObject.name;
    }
}
