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
            Point = Managers.Scenario.NPCs["환자"].transform.GetChild(1).gameObject;
            Point.SetActive(true);
            Point.transform.localScale = new Vector3(0.0764116f, 0.009552589f, 0.07533843f);
            Transform point = Point.transform.GetChild(0); points.Add(point.gameObject);
            point.SetParent(Util.FindChildByName(Managers.Scenario.NPCs["환자"].gameObject, "basic_rig R Forearm").transform, false);
            point.transform.localPosition = new Vector3(-0.0261f, -0.0131f, -0.0409f);
            point.transform.localRotation = Quaternion.Euler(83.043f, -0.499f, -0.359f);
            point.transform.localScale = new Vector3(0.0764116f, 0.01577159f, 0.07533843f);

            point = Point.transform.GetChild(0); points.Add(point.gameObject);
            Point.transform.GetChild(0).SetParent(Util.FindChildByName(Managers.Scenario.NPCs["환자"].gameObject, "basic_rig L Forearm").transform, false);
            point.transform.localPosition = new Vector3(-0.0232f, -0.0232f, 0.0377f);
            point.transform.localRotation = Quaternion.Euler(109.183f, 21.666f, 21.255f);
            point.transform.localScale = new Vector3(0.0764116f, 0.01577159f, 0.07533843f);

            point = Point.transform.GetChild(0); points.Add(point.gameObject);
            Point.transform.GetChild(0).SetParent(Util.FindChildByName(Managers.Scenario.NPCs["환자"].gameObject, "basic_rig L UpperArm").transform, false);
            point.transform.localPosition = new Vector3(0.006689013f, -0.02759328f, 0.01746411f);
            point.transform.localRotation = Quaternion.Euler(90f, 0, 0);
            point.transform.localScale = new Vector3(0.0764116f, 0.01577159f, 0.07533843f);

            point = Point.transform.GetChild(0); points.Add(point.gameObject);
            Point.transform.GetChild(0).SetParent(Util.FindChildByName(Managers.Scenario.NPCs["환자"].gameObject, "basic_rig UpperArm").transform, false);
            point.transform.localPosition = new Vector3(-0.0036f, -0.0311f, -0.0329f);
            point.transform.localRotation = Quaternion.Euler(64.809f, -5.752f, -1.137f);
            point.transform.localScale = new Vector3(0.0764116f, 0.01577159f, 0.07533843f);

            point = Point.transform.GetChild(0); points.Add(point.gameObject);
            Point.transform.GetChild(0).SetParent(Util.FindChildByName(Managers.Scenario.NPCs["환자"].gameObject, "basic_rig R Calf").transform, false);
            point.transform.localPosition = new Vector3(0.0195f, 0.0195f, 0.0005f);
            point.transform.localRotation = Quaternion.Euler(202.019f, 180.416f, 179.297f);
            point.transform.localScale = new Vector3(0.0764116f, 0.01577159f, 0.07533843f);

            point = Point.transform.GetChild(0); points.Add(point.gameObject);
            Point.transform.GetChild(0).SetParent(Util.FindChildByName(Managers.Scenario.NPCs["환자"].gameObject, "basic_rig L Calf").transform, false);
            point.transform.localPosition = new Vector3(0.0195f, 0.0195f, 0.0005f);
            point.transform.localRotation = Quaternion.Euler(21.205f, 47.349f, 14.175f);
            point.transform.localScale = new Vector3(0.0764116f, 0.01577159f, 0.07533843f);

        }

        if (parent == null)
        {
            Debug.LogWarning("Can't find transform");
            return false;
        }

        Managers.Item.SelectedItem.Using = true;
        // 주사기 오브젝트를 손에 붙임
        gameObject.transform.SetParent(parent, false);
        gameObject.transform.localPosition = new Vector3(-0.0985f, 0.0297f, -0.0181f);
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
                if (Input.GetMouseButtonDown(0))
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

                                //clickTime += Time.deltaTime;

                                //// 3초 이상 클릭 유지시
                                //if (clickTime >= holdDuration)
                                //{
                                //    Managers.Scenario.MyAction = "Use";
                                //    if (obj.name != "AccuratePoint")
                                //        Debug.Log("다른게 들어감");

                                //    Managers.Scenario.Targets.Add("AccuratePoint");
                                //    Mycharacter.State = CreatureState.Idle;
                                //    check = true;  // check를 true로 설정하여 완료
                                //}
                                Mycharacter.State = CreatureState.Syringe;
                               
                                StartCoroutine(CoDelayIdle(3.0f));
                                

                            }
                            //else if (clickTime < holdDuration)
                            //{
                            //    Managers.UI.CreateSystemPopup("WarningPopup", "3초가 지나야 합니다.");
                            //}
                        }
                        else if (!Managers.Item.IsInventoryOpen)
                        {
                            Managers.UI.CreateSystemPopup("WarningPopup", "올바른 곳에 사용하세요");
                        }
                    }
                }
                // 마우스 버튼을 떼면 상태를 Idle로 복구
                //else if (Input.GetMouseButtonUp(0))
                //{
                //    if (Mycharacter.State == CreatureState.Syringe)
                //    {
                //        Mycharacter.State = CreatureState.Idle;
                //        clickTime = 0f; // 클릭 시간 초기화
                //    }
                //}
            }
            // 다음 프레임까지 대기
            yield return null;
        }
    }
    IEnumerator CoDelayIdle(float time)
    {

        yield return new WaitForSeconds(time);
        if (obj.name != "AccuratePoint")
        {
            Debug.Log("다른게 들어감");
            yield break;
        }

        Managers.Scenario.Targets.Add("AccuratePoint");
        
        for (int i = 0; i < points.Count; i++)
            points[i].SetActive(false);
        Managers.Item.ForceUnUseItem(ItemInfo);
        Managers.Scenario.MyAction = "Use";
        check = true;  // check를 true로 설정하여 완료
        if(Managers.Item.IsInventoryOpen == true)
        {
            Managers.Item.OpenOrCloseInventory();
        }
       
        Mycharacter.State = CreatureState.Idle;
    }
}
