using Google.Protobuf.Protocol;
using UnityEngine;

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
            Dir = new Vector2(value.DirX, value.DirY);
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

    public Vector2 Dir
    {
        get { return new Vector2(MoveInfo.DirX, MoveInfo.DirY); }
        set
        {
            if (MoveInfo.DirX == value.x && MoveInfo.DirY == value.y)
                return;

            MoveInfo.DirX = value.x;
            MoveInfo.DirY = value.y;
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
            _updated = true;
        }
    }

    #endregion

    protected bool _syncUpdated = false;
    protected bool _updated = false;
    [SerializeField] private float _speed = 1.5f;
    protected Vector3 _destPos = Vector3.zero;

    #endregion

    private void Update()
    {
        UpdateController();
    }

    protected virtual void UpdateController()
    {
        UpdateMove();
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

        State = CreatureState.Run;
        moveDir = moveDir.normalized;
        transform.position = transform.position + (moveDir * _speed * Time.deltaTime);
    }

    protected virtual void UpdateAnimation()
    {

    }

    public void Sync()
    {
        Vector3 _destPos = Pos;
        transform.position = _destPos;
    }
}
