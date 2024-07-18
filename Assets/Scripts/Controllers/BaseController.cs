using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

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
    public CreatureState State
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
    protected GameObject _phone;
    Coroutine _coSync;
    GameObject _positionDisplay;

    #endregion

    protected virtual void Start()
    {
        Place = "스테이션";
        _animator = GetComponent<Animator>();
        GameObject leftHand = Util.FindChildByName(this.gameObject, "L_hand_grap_point");
        _phone = Managers.Resource.Instantiate("Objects/Phone", leftHand.transform);
        _phone.SetActive(false);

        if(Position != null)
        {
            _positionDisplay = Managers.UI.CreateUI("PositionDisplay", UIManager.CanvasType.World);
            _positionDisplay.GetComponent<FloatingUI>().Init(transform, Position, 2.0f);
        }
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
            case CreatureState.CleanTable:
                _animator.Play("clean-table");
                break;
            default:
                Debug.LogError($"There is no animation clip about {State}");
                break;
        }

        ActiveObjectOnState(State);
    }

    void ActiveObjectOnState(CreatureState state)
    {
        if (state == CreatureState.UsingPhone)
            _phone.SetActive(true);
        else
            _phone.SetActive(false);
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

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("Map"))
            return;

        Place = other.gameObject.name;
    }
}
