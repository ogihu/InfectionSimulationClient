using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using static Define;

public class PlayerNPCController : NPCController
{
    //false인 애가 있으면 시나리오 진행 안됨
    public bool PassScenario {  get; set; }
    public int GenerateNumber { get; set; }
    public Vector3 WaitingPoint { get; set; }
    Coroutine _co;

    public override void Awake()
    {
        base.Awake();

        PassScenario = false;
        _agent.stoppingDistance = 5f;
        Place = "스테이션";
    }

    /// <summary>
    /// 본인 시나리오 차례가 아닐 경우, 보호구를 착용하고 있지 않은데 격리실이나 전실일 경우 이동
    /// </summary>
    /// <returns></returns>
    public IEnumerator CoActionByProtects()
    {
        //보호구 착용 안했으면 스테이션으로 복귀
        if (!Items.ContainsKey("Glove"))
        {
            if(_agent.isStopped == true)
                _agent.isStopped = false;

            yield return CoGoPlace("스테이션");
        }
        yield break;
    }

    /// <summary>
    /// place에 도착하면 정지
    /// </summary>
    /// <param name="place"></param>
    /// <returns></returns>
    public IEnumerator CoGoPlace(string place)
    {
        if (string.IsNullOrEmpty(place))
        {
            StopOrder();
            yield break;
        }

        if(place == "스테이션")
            _agent.stoppingDistance = 0.2f;
        else
            _agent.stoppingDistance = 1f;

        if (place == "스테이션")
            SetDestination(WaitingPoint);
        else
            SetDestination(FindPlace(place));

        if (_agent.isStopped == true)
            _agent.isStopped = false;

        yield return new WaitUntil(() => !_agent.pathPending);
        State = CreatureState.Run;

        yield return new WaitUntil(() => _agent.remainingDistance <= _agent.stoppingDistance);

        _agent.velocity = Vector3.zero;
        State = CreatureState.Idle;

        StopOrder();
        yield break;
    }

    /// <summary>
    /// 캐릭터를 추적해서 접근 후, 바라보는 함수
    /// </summary>
    /// <param name="targetCharacter"></param>
    /// <returns></returns>
    public IEnumerator CoGoCharacter(ScenarioInfo info)
    {
        if (info.Targets.Count <= 0)
        {
            StopOrder();
            yield break;
        }

        _agent.stoppingDistance = 4f;
        BaseController target = Managers.Object.Characters[info.Targets[0]];
        SetDestination(target.transform.position);

        if (_agent.isStopped == true)
            _agent.isStopped = false;

        yield return new WaitUntil(() => !_agent.pathPending);
        State = CreatureState.Run;

        while (_agent.remainingDistance > _agent.stoppingDistance)
        {
            SetDestination(target.transform.position);
            yield return new WaitForSeconds(1f);
        }

        _agent.velocity = Vector3.zero;
        State = CreatureState.Idle;
        yield return StartCoroutine(CoLookTarget(target.transform.position));
        yield break;
    }

    public IEnumerator CoGoObject(ScenarioInfo info)
    {
        string objectName = "";

        switch (info.Action)
        {
            case "EMRWrite":
            case "EMRRead":
            case "SCRFWrite":
                objectName = "PC";
                break;
            case "X-Ray":
                objectName = "X-Ray";
                break;
        }

        if (string.IsNullOrEmpty(objectName))
            yield break;

        Transform targetObject = null;
        foreach(var target in Managers.Scenario.InteractableObjectsList)
        {
            if(target.name == objectName)
            {
                targetObject = target.transform;
            }
        }

        if (targetObject == null)
            yield break;

        _agent.stoppingDistance = 1f;
        SetDestination(targetObject.position);

        if (_agent.isStopped == true)
            _agent.isStopped = false;

        yield return new WaitUntil(() => !_agent.pathPending);
        State = CreatureState.Run;
        yield return new WaitUntil(() => _agent.remainingDistance <= _agent.stoppingDistance);

        _agent.velocity = Vector3.zero;
        State = CreatureState.Idle;
        yield return StartCoroutine(CoLookTarget(targetObject.position));
        yield break;
    }

    /// <summary>
    /// 목표물 바라보기
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    public IEnumerator CoLookTarget(Vector3 target)
    {
        Vector3 direction = (target - transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = targetRotation;

        State = CreatureState.Idle;
        StopOrder();
        yield break;
    }

    /// <summary>
    /// time 만큼 대기
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    public IEnumerator CoWait(float time)
    {
        yield return new WaitForSeconds(time);
        State = CreatureState.Idle;
        StopOrder();
        yield break;
    }

    /// <summary>
    /// 말풍선 생성
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public IEnumerator CoChangeChatBubble(string message)
    {
        Managers.UI.ChangeChatBubble(transform, message);
        yield return null;
        StopOrder();
        yield break;
    }

    /// <summary>
    /// 4종 보호구 착용
    /// </summary>
    /// <returns></returns>
    public IEnumerator CoEquipProtects()
    {
        GameObject go = Managers.Resource.Instantiate("Items/FourProtects");
        if (go == null)
        {
            Debug.LogError("No gameObject in Items folder : FourProtects");
            yield break;
        }

        Item item = go.GetComponent<Item>();
        if (item == null)
        {
            Debug.LogError("No component : Item");
            yield break;
        }

        item.Use(this);
        StopOrder();
        yield break;
    }

    /// <summary>
    /// 4종 보호구 해제
    /// </summary>
    /// <returns></returns>
    public IEnumerator CoUnEquipProtects()
    {
        if (!Items.TryGetValue("FourProtects", out var item))
        {
            Debug.LogError("Character don't have item : FourProtects");
            yield break;
        }

        if(item == null)
        {
            Debug.LogError("Character don't have item : FourProtects");
            yield break;
        }    

        Item component = item.GetComponent<Item>();
        if(component == null)
        {
            Debug.LogError("No Item component in this object : FourProtects");
            yield break;
        }

        component.UnUse();
        StopOrder();
        yield break;
    }

    /// <summary>
    /// 아이템 명이 name인 아이템 사용
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public IEnumerator CoUseItem(ScenarioInfo info)
    {
        if (info.Action != "Use")
        {
            StopOrder();
            yield break;
        }

        if (string.IsNullOrEmpty(info.Item))
        {
            Debug.LogError("Item이 데이터에 정의되지 않았습니다.");
            StopOrder();
            yield break;
        }

        string name = info.Item;

        GameObject go = Managers.Resource.Instantiate($"Items/{name}");
        if(go == null)
        {
            Debug.LogError($"There is no prefab in Items folder : {name}");
            yield break;
        }

        Item item = go.GetComponent<Item>();
        if(item == null)
        {
            Debug.LogError($"no component in this item : {name}");
            yield break;
        }

        item.Use(this);
        StopOrder();
        yield break;
    }

    /// <summary>
    /// 아이템 명이 name인 아이템 해제
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    public IEnumerator CoUnUseItem(ScenarioInfo info)
    {
        if (string.IsNullOrEmpty(info.Item))
        {
            Debug.LogError("Item이 데이터에 정의되지 않았습니다.");
            StopOrder();
            yield break;
        }

        string name = info.Item;

        if (!Items.TryGetValue(name, out var obj))
        {
            Debug.LogError($"Character don't have item : {name}");
            yield break;
        }

        Item item = obj.GetComponent<Item>();
        if(item == null)
        {
            Debug.LogError($"No item component in object : {name}");
            yield break;
        }

        item.UnUse();
        StopOrder();
        yield break;
    }

    /// <summary>
    /// 대본이 있으면 말풍선 생성
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    public IEnumerator CoMakeChat(string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            StopOrder();
            yield break;
        }

        Managers.UI.ChangeChatBubble(transform, message);
        StopOrder();
        yield break;
    }

    /// <summary>
    /// 현재 Action에 따른 State 변경 및 애니메이션을 위한 일정 시간 대기, 대기 후 Idle로 상태 변경
    /// </summary>
    /// <param name="info"></param>
    public IEnumerator CoChangeStateAndWait(ScenarioInfo info)
    {
        if (string.IsNullOrEmpty(info.Action))
        {
            Debug.LogError("시나리오 데이터 Action이 없습니다.");
            yield break;
        }

        switch (info.Action)
        {
            case "Tell":
                State = CreatureState.Conversation;
                break;
            case "Call":
            case "Messenger":
                State = CreatureState.UsingPhone;
                break;
            case "Use":
                switch (info.Item)
                {
                    case "Syringe":
                        State = CreatureState.Syringe;
                        break;
                    case "DrySwab":
                        State = CreatureState.DrySwab;
                        break;
                }
                break;
            default:
                State = CreatureState.Idle;
                break;
        }

        yield return StartCoroutine(CoWait(GetAnimationDelayTime(State)));
        yield break;
    }

    /// <summary>
    /// 모든 행동 끝나면 시나리오 수행 완료 전송
    /// </summary>
    /// <returns></returns>
    public IEnumerator CoCompleteScenario()
    {
        PassScenario = true;
        State = CreatureState.Idle;
        Managers.Scenario.CheckPlayerNPCComplete();
        StopOrder();
        yield break;
    }

    public IEnumerator AutoActionScenario(ScenarioInfo info)
    {
        if(_co != null)
        {
            _agent.isStopped = true;
            _agent.velocity = Vector3.zero;
            StopCoroutine(_co);
            _co = null;
        }

        if (info.Position != Position)
        {
            PassScenario = true;
            yield return _co = StartCoroutine(CoActionByProtects());
            yield break;
        }

        if (string.IsNullOrEmpty(info.Action))
        {
            Debug.LogError("시나리오 데이터에 Action이 없습니다.");
            yield break;
        }

        switch (info.Action)
        {
            case "Tell":
                yield return CoGoPlace(info.Place);
                yield return CoGoCharacter(info);
                yield return CoChangeStateAndWait(info);
                yield return CoMakeChat(info.OriginalSentence);
                break;
            case "Call":
            case "Messenger":
                yield return CoGoPlace(info.Place);
                yield return CoChangeStateAndWait(info);
                yield return CoMakeChat(info.OriginalSentence);
                break;
            case "Use":
                switch (info.Item)
                {
                    case "Syringe":
                    case "DrySwab":
                        yield return CoGoPlace(info.Place);
                        yield return CoUseItem(info);
                        yield return CoGoCharacter(info);
                        yield return CoChangeStateAndWait(info);
                        yield return CoUnUseItem(info);
                        break;
                    default:
                        yield return CoGoPlace(info.Place);
                        yield return CoUseItem(info);
                        yield return CoGoCharacter(info);
                        yield return CoChangeStateAndWait(info);
                        break;
                }
                break;
            case "UnUse":
                yield return CoGoPlace(info.Place);
                yield return CoUnUseItem(info);
                yield return CoChangeStateAndWait(info);
                break;
            case "EMRWrite":
            case "EMRRead":
            case "SCRFWrite":
                yield return CoGoPlace(info.Place);
                yield return CoGoObject(info);
                yield return CoChangeStateAndWait(info);
                break;
            case "X-Ray":
                yield return CoGoPlace(info.Place);
                yield return CoGoObject(info);
                yield return CoChangeStateAndWait(info);
                break;
            default:
                yield return CoGoPlace(info.Place);
                yield return CoChangeStateAndWait(info);
                break;
        }

        yield return CoCompleteScenario();
        yield break;
    }

    Vector3 FindPlace(string place)
    {
        return Managers.Map.FindPlacePosition(place);
    }
}
