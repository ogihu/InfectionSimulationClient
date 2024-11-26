using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OrderCheckButton : MonoBehaviour
{
    public Button myButton; // 연결할 버튼
    MPX_Clothing_Panel mPX_Clothing_Panel;
    void Start()
    {
        mPX_Clothing_Panel = transform.parent.parent.gameObject.GetComponent<MPX_Clothing_Panel>();
        myButton = gameObject.GetComponent<Button>();
        // 버튼 클릭 이벤트에 메서드 연결
        myButton.onClick.AddListener(OnButtonClick);
    }

    // 버튼 클릭 시 실행할 메서드
    void OnButtonClick()
    {
        if (gameObject.name == "Answer")
        {
            mPX_Clothing_Panel.CheckOrder();
        }
            
        else if (gameObject.name == "Reset")
        {
            GameObject go;
            mPX_Clothing_Panel.child = Managers.UI.CreateUI(Managers.Scenario.CurrentScenarioInfo.Action, mPX_Clothing_Panel.gameObject.transform);
            go = Util.FindChild(mPX_Clothing_Panel.child, "Order_view");
            mPX_Clothing_Panel.checkingCount = 0;
            mPX_Clothing_Panel.RandomSlot(go);
            Destroy(transform.parent.gameObject);
        }
        
    }
}
