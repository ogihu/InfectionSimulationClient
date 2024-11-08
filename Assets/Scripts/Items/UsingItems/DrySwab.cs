using Google.Protobuf.Protocol;
using Ricimi;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;
using static Google.Cloud.Speech.V1.LanguageCodes;

public class DrySwab : UsingItem
{
    Texture2D cursorTexture;  // 사용할 커서 이미지
    GameObject popup;
    GameObject lesion;
    GameObject obj;
    public bool check = false;

    public override bool Use(BaseController character)
    {
        check = false;
        cursorTexture = Resources.Load<Texture2D>("Sprites/itemIcons/Use_DrySwab");
        StartCoroutine(StartChecking());
        return base.Use(character);
    }

    IEnumerator StartChecking()
    {
        int i = 0;
        popup = Managers.UI.CreateUI("PopupNotice");

        //@@@@@@@@@@이 부분 환자 클릭하면 면봉 사용 UI 표시되게 해야함. 혈액채취 부분도 마찬가지@@@@@@@@@@
        StartCoroutine(StartChecking());
        //카운트다운 시작
        for (i = 3; i > 0; i--)
        {
            if (popup == null)
                Debug.Log("이거 안됨");
            popup.transform.GetChild(0).GetComponent<TMP_Text>().alignment = TextAlignmentOptions.Center;   //중앙정렬
            popup.transform.GetChild(0).GetComponent<TMP_Text>().text = i.ToString() + "초 뒤에 병변 이미지가 주어집니다.";
            yield return new WaitForSeconds(1f); // 1초 대기
        }
        Managers.UI.DestroyUI(popup);

        Managers.Scenario.State_Image = true;
        lesion = Managers.UI.CreateUI("LesionUI");
        lesion.GetComponent<LesionUI>().Swab = this;

        //면봉 상태(애니메이션)
        //Mycharacter.State = CreatureState.Syringe;

        Vector2 hotSpot = new Vector2(0, cursorTexture.height);
        Cursor.SetCursor(cursorTexture, hotSpot, CursorMode.ForceSoftware);
    }

    public void CompleteLesion()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        Managers.Scenario.State_Image = false;
        Managers.Item.ForceDropItem(ItemInfo);
        Managers.Scenario.MyAction = "Use";
        Managers.Scenario.Item = "DrySwab";
        Managers.Scenario.Targets.Add("환자");
    }
    IEnumerator PassCheck()
    {
        // 면봉 캐릭터 손에 부여
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
                            check = true; 
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
}
