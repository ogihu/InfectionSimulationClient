using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BaseController : MonoBehaviour
{
    #region Fields

    public int Id { get; set; }

    public string Name { get; set; }

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

    protected Animator _animator;
    protected bool _syncUpdated = false;
    protected bool _updated = false;
    [SerializeField] private float _speed = 1.5f;
    protected Vector3 _destPos = Vector3.zero;
    [Tooltip("MyPlayer는 적용되지 않음")]
    [SerializeField] private float _rotationSpeed;
    Coroutine _coSync;


    #endregion

    protected virtual void Start()
    {
        _animator = GetComponent<Animator>();
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
        if(InputBit == 0)
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

        _coSync = null;
        State = CreatureState.Run;
        moveDir = moveDir.normalized;
        transform.Translate(moveDir * _speed * Time.deltaTime);
    }

    protected virtual void UpdateRotation()
    {
        Quaternion targetRotation = Quaternion.LookRotation(Dir);

        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * _rotationSpeed);
    }


    protected virtual void UpdateAnimation()
    {
        switch (State)
        {
            case CreatureState.Idle:
                _animator.Play("idle");
                break;
            case CreatureState.Run:
                _animator.Play("run");
                break;
            default:
                Debug.LogError($"There is no animation clip about {State}");
                break;
        }
    }
<<<<<<< Updated upstream
=======

    public void UpdateSync()
    {
        float distance = (Pos - transform.position).magnitude;

        if (distance > 2.0f)
            ImmediateSync();
        else
        {
            _coSync = StartCoroutine(CoSync());
        }
    }

    public void ImmediateSync()
    {
        if(transform.position != Pos)
            transform.position = Pos;
    }
    
    IEnumerator CoSync()
    {
        yield return new WaitUntil(() => State == CreatureState.Idle);

        Vector3 dir = (Pos - transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(dir);
        transform.LookAt(dir);

        while (true)
        {
            transform.Translate(dir * _speed * Time.deltaTime);
            yield return null;
        }
    }
>>>>>>> Stashed changes
}
