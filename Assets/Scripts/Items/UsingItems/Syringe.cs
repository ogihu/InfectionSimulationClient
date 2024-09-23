using Google.Protobuf.Protocol;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.TextCore.Text;
public class Syringe : UsingItem
{
    Transform parent;
    public override bool Use(BaseController character)
    {
        //주사기 오브젝트 손에 생성
        //채혈 시작
        //캐릭터 상태 UsingItem? 같은 상태로 변경

        //if (gameObject != null)
        //    return false;
        parent = Util.FindChildByName(character.gameObject, "basic_rig L Hand").transform;

        if (parent == null)
        {
            Debug.LogWarning("Can't find transform");
        }

        gameObject.transform.SetParent(parent, false);

        // 마우스 클릭을 감지하는 반복문
        if (Input.GetMouseButtonDown(0) && gameObject != null)
        {
            if (gameObject != null && character.GetComponent<MyPlayerController>()._interactionObject.transform.gameObject.name == "AccuratePoint")
            {
                character.State = CreatureState.Syringe;
                if (Managers.Scenario.CurrentScenarioInfo != null)
                {
                    Managers.Scenario.MyAction = "BloodCollection";
                }
            }
            else
            {
                Debug.Log("다시 시도하세요.");
            }
        }
        gameObject.transform.localPosition = new Vector3(-0.0751f, 0.0215f, 0.0073f);
        gameObject.transform.localRotation =  Quaternion.Euler(-0.215f, 133.05f, -82.805f);
        gameObject.transform.localScale = new Vector3(0.8333f,0.8333f,0.8333f);
        return base.Use(character);
    }

}
