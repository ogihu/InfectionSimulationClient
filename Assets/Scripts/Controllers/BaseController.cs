using Google.Protobuf.Protocol;
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

            AccountId = value.AccountId;
            Position = value.Position;
        }
    }

    public string AccountId
    {
        get { return UserInfo.AccountId; }
        set
        {
            if (UserInfo.AccountId.Equals(value))
                return;

            UserInfo.AccountId = value;
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
    public virtual string Place { get; set; }
    protected GameObject _usingItem;
    Coroutine _coSync;
    public GameObject _positionDisplay;
    public Dictionary<string, GameObject> Items = new Dictionary<string, GameObject>();

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
        target.AccountId = this.AccountId;
        target.Position = this.Position;
        target.Items = this.Items;
    }

    public virtual void Awake()
    {
        _animator = GetComponent<Animator>();

        _audioSource = GetComponent<AudioSource>();

        _leftHand = Util.FindChildByName(this.gameObject, "L_hand_grap_point").transform;
        _rightHand = Util.FindChildByName(this.gameObject, "R_hand_grap_point").transform;

        Managers.UI.CreateChatBubble(this.transform);
    }

    public virtual void Start()
    {
        if (Position != null)
        {
            _positionDisplay = Managers.UI.CreateUI("PositionDisplay", UIManager.CanvasType.World);
            _positionDisplay.GetComponent<FloatingUI>().Init(transform, y: 2.0f);
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

    /// <summary>
    /// state에 재생되는 애니메이션 클립의 이름 반환
    /// </summary>
    /// <param name="state"></param>
    /// <returns></returns>
    protected string GetAnimationByState(CreatureState state)
    {
        switch (state)
        {
            case CreatureState.Idle:
            case CreatureState.Setting:
            case CreatureState.UsingInventory:
            case CreatureState.Emr:
                return "Idle";

            case CreatureState.Run:
                return "Run";

            case CreatureState.Conversation:
                return "Conversation";

            case CreatureState.UsingPhone:
                return "UsingPhone";

            case CreatureState.Clean:
                return "Clean";

            case CreatureState.PickUp:
                return "PickUp";

            case CreatureState.Push:
                return "Push";

            case CreatureState.Sit:
                return "Sit";

            case CreatureState.Typing:
                return "Typing";

            case CreatureState.SweepingFloor:
                return "SweepingFloor";

            case CreatureState.UsingTissue:
                return "UsingTissue";

            case CreatureState.LyingIdle:
                return "LyingIdle";

            case CreatureState.Syringe:
                return "Syringe";

            case CreatureState.Blocking:
                return "Blocking";

            case CreatureState.DrySwab:
                return "DrySwab";
            default:
                return "Idle";
        }
    }

    /// <summary>
    /// clipName의 애니메이션 클립을 찾아 반환
    /// </summary>
    /// <param name="clipName"></param>
    /// <returns></returns>
    protected AnimationClip GetAnimationClipByName(string clipName)
    {
        if(_animator == null || _animator.runtimeAnimatorController == null)
        {
            _animator = GetComponent<Animator>();
        }

        AnimationClip[] clips = _animator.runtimeAnimatorController.animationClips;
        foreach(var clip in clips)
        {
            if(clip.name == clipName)
            {
                return clip;
            }
        }

        return null;
    }

    /// <summary>
    /// state일 때의 애니메이션 클립의 재생시간을 반환(loop일 경우 고정 반환)
    /// </summary>
    /// <param name="state"></param>
    /// <returns></returns>
    protected float GetAnimationDelayTime(CreatureState state)
    {
        string clipName = GetAnimationByState(state);
        AnimationClip clip = GetAnimationClipByName(clipName);

        if (clip.isLooping)
            return 2.0f;
        else
            return clip.length;
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

    public bool UseItem(GameObject item)
    {
        if(item.GetComponent<Item>() as Equipment)
        {
            if (Items.ContainsKey(item.name))
                return false;

            Items.Add(item.name, item);

            if (item.GetComponent<Animator>() != null)
                _equipmentAnimator.Add(item.GetComponent<Animator>());

            return true;
        }
        else if(item.GetComponent<Item>() as UsingItem)
        {
            if (Items.ContainsKey(item.name))
                return false;

            Items.Add(item.name, item);

            if (item.GetComponent<Animator>() != null)
                _equipmentAnimator.Add(item.GetComponent<Animator>());

            return true;
        }
        else if (item.GetComponent<Item>() as ImmediatelyUsingItem)
        {
            return true;
        }

        return false;
    }

    public void UnUseItem(string item)
    {
        if (Items.ContainsKey(item))
        {
            GameObject go = Items[item];
            Items.Remove(item);

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

    public void Clear()
    {
        Managers.UI.DestroyUI(_positionDisplay);
        _positionDisplay = null;
        Items.Clear();
    }
}
