using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.AI;

public class NPCController : CreatureController
{
    protected NavMeshAgent _agent;
    protected Transform _target;
    protected Coroutine _order;
    protected Queue<IEnumerator> _orderQueue = new Queue<IEnumerator>();

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
        if (_order != null)
            return;

        if (_orderQueue.Count > 0)
        {
            _order = StartCoroutine(_orderQueue.Dequeue());
            return;
        }

        //if (_agent.velocity != Vector3.zero)
        //{
        //    State = CreatureState.Run;
        //    return;
        //}
    }

    protected override void UpdateRotation()
    {
        //if (State != CreatureState.Idle)
        //    return;

        //if (Managers.Object.MyPlayer == null)
        //    return;

        //Vector3 dir = Managers.Object.MyPlayer.transform.position - transform.position;

        //Quaternion targetRotation = Quaternion.LookRotation(dir);
        //transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * _rotationSpeed);
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
        StopOrder();
        State = CreatureState.Idle;
    }

    public IEnumerator CoGoDestination_Animation(Vector3 point, CreatureState animation)
    {
        SetDestination(point);
        yield return new WaitUntil(() => !_agent.pathPending);
        State = animation;
        yield return new WaitUntil(() => _agent.remainingDistance <= _agent.stoppingDistance);
        _agent.velocity = Vector3.zero;
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

    public IEnumerator CoTeleport(Vector3 point)
    {
        Teleport(point);
        StopOrder();
        yield break;
    }

    public IEnumerator CoSetForward(Vector3 forward)
    {
        Quaternion targetRotation = Quaternion.LookRotation(forward);
        transform.rotation = targetRotation;
        StopOrder();
        yield break;
    }

    public IEnumerator CoSetState(CreatureState state)
    {
        if (_agent.velocity != Vector3.zero)
        {
            _agent.velocity = Vector3.zero;
        }

        State = state;
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

    protected void SetDestination(Vector3 point)
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
            Items[equipment].GetComponent<Equipment>().UnUse();
    }

    public void SetState(CreatureState state)
    {
        if (_agent.velocity != Vector3.zero)
        {
            _agent.velocity = Vector3.zero;
        }

        State = state;
    }

    public bool Teleport(Vector3 point)
    {
        if(point == Define.WaitingArea)
        {
            Pos = new Vector3(point.x, point.y, point.z - (1.5f * Define.WaitingCount++));
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

    public void FreezePosition()
    {
        Rigidbody rb = _agent.GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeAll;
    }

    public void ResetSpeed()
    {
        ChangeSpeed(6);
    }

    public void ChangeSpeed(float value)
    {
        _agent.speed = value;
    }
}
