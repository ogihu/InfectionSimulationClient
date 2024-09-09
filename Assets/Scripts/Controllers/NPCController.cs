using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.AI;

public class NPCController : CreatureController
{
    NavMeshAgent _agent;
    Transform _target;
    Coroutine _order;
    Queue<IEnumerator> _orderQueue = new Queue<IEnumerator>();

    public override void Awake()
    {
        base.Awake();
        _agent = GetComponent<NavMeshAgent>();
        _agent.stoppingDistance = 2.0f;
        _target = null;
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("NPC"), LayerMask.NameToLayer("Player"), true);
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("NPC"), LayerMask.NameToLayer("MyPlayer"), true);
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
        if (_order == null)
            return;

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

    public void Equip(string equipment)
    {
        GameObject eqm = Managers.Resource.Instantiate($"Equipments/{equipment}");
        Equipment component = eqm.GetComponent<Equipment>();
        
        if (component == null)
            return;

        component.Use(this);
    }

    public void UnEquip(string equipment)
    {
        if(Equipment.ContainsKey(equipment))
            Equipment[equipment].GetComponent<Equipment>().UnUse(this);
    }

    public bool IsWorking()
    {
        if (State == CreatureState.Idle)
            return false;

        return true;
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

    public void DestoyRb()
    {
        Rigidbody rb = _agent.GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeAll;
        _agent.GetComponent<NPCController>().enabled = false;
        
    }

    public void AddController()
    {
        _agent.GetComponent<NPCController>().enabled = true;
    }

    public IEnumerator CoGoDestination_Animation(Vector3 point , CreatureState animation)
    {
        SetDestination(point);
        yield return new WaitUntil(() => _agent.remainingDistance <= _agent.stoppingDistance);
        SetState(animation);
        StopOrder();
    }

    public void ResetSpeed()
    {
        ChangeSpeed(3);
    }

    public void ChangeSpeed(float value)
    {
        _agent.speed = value;
    }
}
