using Ricimi;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Google.Cloud.Speech.V1.LanguageCodes;

public class DrySwab : UsingItem
{
    Texture2D cursorTexture;  // 사용할 커서 이미지
    GameObject popup;
    GameObject lesion;
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
}
