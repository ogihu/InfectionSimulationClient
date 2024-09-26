using Google.Protobuf.Protocol;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.PerformanceData;
using System.Linq;
using UnityEngine.EventSystems;

public class Syringe : UsingItem
{
    Transform parent;
    GameObject go;      // 자체 오브젝트
    GameObject obj;     // 타겟 오브젝트
    BaseController Mycharacter;     // 나 자신
    bool check = false;
    float raycastDistance = 2f;
    int _layerPoint;

    // 마우스 클릭 지속 시간을 추적할 변수
    private float clickTime = 0f;
    // 원하는 시간이 지나면 실행할 함수
    public float holdDuration = 3f;

    public override void Init()
    {
        base.Init();
        _layerPoint = 1 << LayerMask.NameToLayer("Point");
    }

    public override bool Use(BaseController character)
    {
        // 주사기 오브젝트를 캐릭터 손에 생성
        parent = Util.FindChildByName(character.gameObject, "basic_rig L Hand").transform;
        check = false;

        if (Mycharacter == null)
            Mycharacter = character;
        if (go == null)
        {
            go = gameObject;
            Managers.Scenario.NPCs["환자"].transform.GetChild(1).gameObject.SetActive(true);
        }

        if (parent == null)
        {
            Debug.LogWarning("Can't find transform");
            return false;
        }

        Managers.Item.SelectedItem.Using = true;
        // 주사기 오브젝트를 손에 붙임
        gameObject.transform.SetParent(parent, false);
        gameObject.transform.localPosition = new Vector3(-0.0751f, 0.0215f, 0.0073f);
        gameObject.transform.localRotation = Quaternion.Euler(-0.215f, 133.05f, -82.805f);
        gameObject.transform.localScale = new Vector3(0.8333f, 0.8333f, 0.8333f);

        // 코루틴 시작, check가 true가 될 때까지 기다림
        Managers.Instance.StartCoroutine(WaitForCheck());
        return base.Use(character);
    }

    IEnumerator WaitForCheck()
    {
        // check가 true가 될 때까지 SyringeUseCheck를 실행하고 기다림
        yield return StartCoroutine(SyringeUseCheck());

        // check가 true가 된 후 추가 작업
        Debug.Log("check가 true가 되어 다음 작업을 수행합니다.");
    }

    IEnumerator SyringeUseCheck()
    {
        while (!check)
        {
            if (go != null)
            {
                // 마우스 좌클릭 유지 중
                if (Input.GetMouseButton(0))
                {
                    Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);

                    RaycastHit hitInfo;
                    _layerPoint = LayerMask.GetMask("Point");
                    //자식 오브젝트에 리지드바디 안달려있으면 부모껄로 인식됨
                    if (Physics.Raycast(ray, out hitInfo, raycastDistance, _layerPoint))
                    {
                        obj = hitInfo.transform.gameObject;
                        Debug.Log(obj);
                    }
                    else
                    {
                        if (obj != null)
                        {
                            obj = null;
                        }
                    }
                    // 시나리오 액션이 "Use"일 때
                    if (Managers.Scenario.CurrentScenarioInfo.Action == "Use")
                    {
                        // 올바른 오브젝트 클릭 체크
                        if (obj != null && obj.transform.gameObject.name == "AccuratePoint")
                        {
                            // 시나리오 정보가 유효할 때
                            if (Managers.Scenario.CurrentScenarioInfo != null)
                            {
                                Mycharacter.State = CreatureState.Syringe;
                                clickTime += Time.deltaTime;

                                // 3초 이상 클릭 유지시
                                if (clickTime >= holdDuration)
                                {
                                    Managers.Scenario.MyAction = "Use";
                                    if (obj.name != "AccuratePoint")
                                        Debug.Log("다른게 들어감");

                                    Managers.Scenario.Targets.Add("AccuratePoint");
                                    Mycharacter.State = CreatureState.Idle;
                                    check = true;  // check를 true로 설정하여 완료
                                    Managers.Scenario.NPCs["환자"].transform.GetChild(1).gameObject.SetActive(false);
                                }
                            }
                            else if (clickTime < holdDuration)
                            {
                                Managers.UI.CreateSystemPopup("WarningPopup", "3초가 지나야 합니다.");
                            }
                        }
                        else if (!(Mycharacter.State == CreatureState.UsingInventory))
                        {
                            Managers.UI.CreateSystemPopup("WarningPopup", "올바른 곳에 사용하세요");
                        }
                    }
                }
                // 마우스 버튼을 떼면 상태를 Idle로 복구
                else if (Input.GetMouseButtonUp(0))
                {
                    if (Mycharacter.State == CreatureState.Syringe)
                    {
                        Mycharacter.State = CreatureState.Idle;
                        clickTime = 0f; // 클릭 시간 초기화
                    }
                }
            }

            // 다음 프레임까지 대기
            yield return null;
        }
    }

}
