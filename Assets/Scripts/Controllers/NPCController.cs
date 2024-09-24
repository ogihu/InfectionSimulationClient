using Google.Protobuf.Protocol;
using System;
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
        _agent.stoppingDistance = 0.1f;
        _target = null;
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("NPC"), LayerMask.NameToLayer("Player"), true);
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("NPC"), LayerMask.NameToLayer("MyPlayer"), true);
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("NPC"), LayerMask.NameToLayer("NPC"), true);
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("NPC"), LayerMask.NameToLayer("Default"), true);
    }

    protected override void UpdateMove()
    {
        if (!(State == CreatureState.Idle || State == CreatureState.Run))
            return;

        if (_orderQueue.Count > 0)
        {
            if (_order != null)
                return;

            _order = StartCoroutine(_orderQueue.Dequeue());
            return;
        }

        if (_agent.velocity != Vector3.zero)
        {
            State = CreatureState.Run;
            return;
        }

        State = CreatureState.Idle;
    }

    protected override void UpdateRotation()
    {
        if (State != CreatureState.Idle)
            return;

        Vector3 dir = Managers.Object.MyPlayer.transform.position - transform.position;

        Quaternion targetRotation = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * _rotationSpeed);
    }

    public void AddOrder(IEnumerator enumerator)          
    {
        _orderQueue.Enqueue(enumerator);
    }

    #region NPC 제어 기능

    public IEnumerator CoGoDestination(Vector3 point)
    {
        SetDestination(point);
        yield return new WaitUntil(() => !_agent.pathPending);
        State = CreatureState.Run;
        yield return new WaitUntil(() => _agent.remainingDistance <= _agent.stoppingDistance);
        _agent.velocity = Vector3.zero;
        _agent.isStopped = true;
        StopOrder();
    }

    public IEnumerator CoFollow(Transform target)
    {
        if (target == null)
            yield break;

        SetTarget(target);
        while (true)
        {
            SetDestination(_target.position);
            yield return new WaitForSeconds(1.0f);

            if (Vector3.Distance(transform.position, _target.position) <= _agent.stoppingDistance || _target == null)
                break;
        }

        StopOrder();
    }

    public IEnumerator CoUse(string itemName, Action action = null)
    {
        Use(itemName);
        if (action != null)
            action.Invoke();
        StopOrder();
        yield break;
    }

    public IEnumerator CoUnUse(string itemName)
    {
        UnUse(itemName);
        StopOrder();
        yield break;
    }

    public void StopOrder()
    {
        if (_order == null)
            return;

        StopCoroutine(_order);
        _order = null;
        _target = null;
    }

    void SetDestination(Vector3 point)
    {
        Pos = point;
        _agent.SetDestination(Pos);
    }

    public void SetTarget(Transform target)
    {
        _target = target;
    }

    public void SetForward(Vector3 forward)
    {
        Quaternion targetRotation = Quaternion.LookRotation(forward);
        transform.rotation = targetRotation;
    }

    public void Use(string equipment)
    {
        GameObject eqm = Managers.Resource.Instantiate($"Items/{equipment}");
        Item component = eqm.GetComponent<Item>();

        if (component == null)
            return;

        component.Use(this);
    }

    public void UnUse(string equipment)
    {
        if (Items.ContainsKey(equipment))
            Items[equipment].GetComponent<Equipment>().UnUse(this);
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
        if(point == Define.WaitingArea)
        {
            Pos = new Vector3(point.x, point.y, point.z - (6 * Define.WaitingCount++));
        }
        else
        {
            Define.WaitingCount--;
            Pos = point;
        }
        return _agent.Warp(Pos);
    }

    #endregion

    public bool IsWorking()
    {
        if (State == CreatureState.Idle)
            return false;

        return true;
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
