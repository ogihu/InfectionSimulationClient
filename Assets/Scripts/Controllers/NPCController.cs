using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPCController : CreatureController
{
    NavMeshAgent _agent;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
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

    public void SetDestination(Vector3 point)
    {
        Pos = point;
        _agent.SetDestination(Pos);
        State = CreatureState.Idle;
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
