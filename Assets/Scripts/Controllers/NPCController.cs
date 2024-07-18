using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPCController : CreatureController
{
    NavMeshAgent _agent;
    Transform _target;
    Coroutine _order;
    Queue<IEnumerator> _orderQueue = new Queue<IEnumerator>();

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _agent.stoppingDistance = 2.0f;
        _target = null;
    }

    protected override void UpdateMove()
    {
        if (!(State == CreatureState.Idle || State == CreatureState.Run))
            return;

        if(_orderQueue.Count > 0)
        {
            if (_order != null)
                return;

            _order = StartCoroutine(_orderQueue.Dequeue());
            return;
        }

        if (_agent.velocity == Vector3.zero)
        {
            State = CreatureState.Idle;
            return;
        }

        State = CreatureState.Run;
    }

    protected override void UpdateRotation()
    {
        if (State != CreatureState.Idle)
            return;

        Vector3 dir = Managers.Object.MyPlayer.transform.position - transform.position;
        Quaternion targetRotation = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * _rotationSpeed);
    }

    public void SetOrder(IEnumerator enumerator)
    {
        _orderQueue.Enqueue(enumerator);
    }

    #region NPC 명령 수행 기능

    public IEnumerator CoGoDestination(Vector3 point)
    {
        SetDestination(point);
        yield return new WaitUntil(() => _agent.remainingDistance <= _agent.stoppingDistance);
        SetState(CreatureState.Idle);
        StopOrder();
    }

    public IEnumerator CoFollow(Transform target)
    {
        if (target == null)
            yield break;

        SetFollow(target);
        while (true)
        {
            SetDestination(_target.position);
            yield return new WaitForSeconds(1.0f);
        }
    }

    public void StopOrder()
    {
        StopCoroutine(_order);
        _order = null;
    }

    void SetDestination(Vector3 point)
    {
        Pos = point;
        _agent.SetDestination(Pos);
    }

    void SetFollow(Transform target)
    {
        _target = target;
    }

    #endregion

    public void asdf()
    {
        NavMeshPath path = _agent.path;
    }

    public void SetState(CreatureState state)
    {
        if (_agent.velocity != Vector3.zero)
        {
            _agent.velocity = Vector3.zero;
            _agent.isStopped = true;
        }

        State = state;
    }

    public bool Teleport(Vector3 point)
    {
        Pos = point;
        return _agent.Warp(Pos);
    }
}
