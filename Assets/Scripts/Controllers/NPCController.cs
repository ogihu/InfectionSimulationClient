using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPCController : CreatureController
{
    NavMeshAgent _agent;
    Transform _target;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _agent.stoppingDistance = 2.0f;
        _target = null;
    }

    protected override void UpdateController()
    {
        base.UpdateController();
        UpdateAgent();
    }

    protected override void UpdateMove()
    {
        if (!(State == CreatureState.Idle || State == CreatureState.Run))
            return;

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

    void UpdateAgent()
    {
        UpdateFollow();
    }

    public void UpdateFollow()
    {
        if (_target == null)
            return;

        _agent.SetDestination(_target.position);
    }

    public void SetDestination(Vector3 point)
    {
        Pos = point;
        _agent.SetDestination(Pos);
        State = CreatureState.Idle;
    }

    public void SetFollow(Transform target = null)
    {
        _target = target;
    }

    IEnumerator CoCleanObject(Transform target)
    {
        if(target == null)
            yield break;

        SetDestination(target.position);
        yield return new WaitUntil(() => (target.position - gameObject.transform.position).magnitude < 2f);
        SetState(CreatureState.CleanTable);
    }

    public void SetState(CreatureState state)
    {
        if(_agent.velocity != Vector3.zero)
        {
            _agent.velocity = Vector3.zero;
            _agent.isStopped = true;
        }

        State = state;
    }

    public void Teleport(Vector3 point)
    {
        Pos = point;
        ImmediateSync();
    }
}
