using Google.Protobuf.Protocol;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.EventSystems;
using System.Drawing;

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
    GameObject Point;
    List<GameObject> points = new List<GameObject>();

    public override void Init()
    {
        base.Init();
    }

    public override bool Use(BaseController character)
    {
        // 주사기 오브젝트를 캐릭터 손에 생성
        parent = Util.FindChildByName(character.gameObject, "basic_rig L Hand").transform;

        // 주사기 오브젝트를 손에 붙임
        gameObject.transform.SetParent(parent, false);
        gameObject.transform.localPosition = new Vector3(-0.0985f, 0.0297f, -0.0181f);
        gameObject.transform.localRotation = Quaternion.Euler(-0.215f, 133.05f, -82.805f);
        gameObject.transform.localScale = new Vector3(0.8333f, 0.8333f, 0.8333f);

        if (Managers.Object.MyPlayer != character)
            return true;

        check = false;
        go = gameObject;

        if (Mycharacter == null)
            Mycharacter = character;
        

        Managers.Item.SelectedItem.Using = true;

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
            if (Input.GetMouseButtonDown(0))
            {
                // 바라보는 대상 확인
                RaycastHit hit;
                if (Physics.Raycast(transform.position, transform.forward, out hit))
                {
                    // 바라보는 대상의 이름이 "환자"일 경우
                    if (hit.transform.gameObject.name == "환자")
                    {
                        obj = hit.transform.gameObject;
                    }
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
                    if (obj != null && obj.transform.gameObject.name == "환자")
                    {
                        // 시나리오 정보가 유효할 때
                        if (Managers.Scenario.CurrentScenarioInfo != null)
                        {
                            Mycharacter.State = CreatureState.Syringe;

                            StartCoroutine(CoDelayIdle(3.0f));
                        }
                    }
                    else if (!Managers.Item.IsInventoryOpen)
                    {
                        Managers.UI.CreateSystemPopup("WarningPopup", "올바른 곳에 사용하세요.", UIManager.NoticeType.Warning);
                    }
                }
            }
           // 다음 프레임까지 대기
           yield return null;
        }
    }
    IEnumerator CoDelayIdle(float time)
    {
        yield return new WaitForSeconds(time);

        Managers.Item.ForceDropItem(ItemInfo);
        Managers.Scenario.MyAction = "Use";
        check = true;  // check를 true로 설정하여 완료
        Managers.Scenario.Targets.Add("환자");
        if(Managers.Item.IsInventoryOpen == true)
        {
            Managers.Item.OpenOrCloseInventory();
        }
       
        Mycharacter.State = CreatureState.Idle;
    }
}
