using Google.Protobuf.Protocol;
using UnityEngine;

public class BaseController : MonoBehaviour
{
    #region Fields

    public int Id { get; set; }

    public string Name { get; set; }

    #region PositionInfo PosInfo

    PositionInfo _positionInfo = new PositionInfo();
    public PositionInfo PosInfo
    {
        get { return _positionInfo; }
        set
        {
            if (_positionInfo.Equals(value))
                return;

            State = value.State;
            Pos = new Vector3(value.PosX, value.PosY, value.PosZ);
        }
    }

    //캐릭터 상태
    public CreatureState State
    {
        get { return PosInfo.State; }
        set
        {
            if (PosInfo.State == value)
                return;

            PosInfo.State = value;
            _updated = true;
        }
    }

    //캐릭터 위치
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

    protected bool _updated = false;

    #endregion

    [SerializeField] private float _speed = 1.5f;
    protected Vector3 _destPos = Vector3.zero;

    #endregion

    private void Update()
    {
        UpdateController();
    }

    protected virtual void UpdateController()
    {
        switch (State)
        {
            case CreatureState.Idle:
                UpdateIdle();
                break;
            case CreatureState.Moving:
                UpdateMoving();
                break;
        }
    }

    protected virtual void UpdateIdle() 
    {
        if(transform.position != Pos)
            State = CreatureState.Moving;
    }

    protected virtual void MoveToNextPos() { }

    protected virtual void UpdateMoving()
    {
        _destPos = Pos;

        if ((_destPos - transform.position).magnitude > _speed * Time.deltaTime)
        {
            transform.position += (_destPos - transform.position).normalized * _speed * Time.deltaTime;
            State = CreatureState.Moving;
        }
        else
        {
            transform.position = _destPos;
            State = CreatureState.Idle;
        }
    }

    public void Sync()
    {
        Vector3 _destPos = Pos;
        transform.position = _destPos;
    }
}
