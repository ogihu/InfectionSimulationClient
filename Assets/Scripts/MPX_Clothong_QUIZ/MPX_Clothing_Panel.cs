using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MPX_Clothing_Panel : MonoBehaviour
{
    public int checkingCount = 0;
    GameObject Order;
    GameObject Order_view;
    FourProtects fourProtects;
    public GameObject child;
    List<Vector2> list = new List<Vector2>();

    private void Awake()
    {
        child = Managers.UI.CreateUI(Managers.Scenario.CurrentScenarioInfo.Action, gameObject.transform);
        Order = Util.FindChild(gameObject.transform.GetChild(0).gameObject, "Order");
        fourProtects = Managers.Resource.Instantiate($"Items/FourProtects").GetComponent< FourProtects>();
        Managers.Quiz.MPX_Clothing_Panel_opencheck = true;
        RandomSlot();
    }

    public void Open_MPX_Panel()
    {
        if (Managers.Scenario.CurrentScenarioInfo.Place != Managers.Object.MyPlayer.Place)
        {
            Managers.UI.CreateSystemPopup("WarningPopup", "해당 구역이 아닙니다.", UIManager.NoticeType.Warning);
            return;
        }

        child = Managers.UI.CreateUI(Managers.Scenario.CurrentScenarioInfo.Action, gameObject.transform);
        Managers.Quiz.MPX_Clothing_Panel_opencheck = true;
        RandomSlot();
    }

    public void CloseMPX_Panel()
    {
        Managers.UI.DestroyUI(gameObject);
    }

    public void CheckOrder()
    {
        if(Order == null)
            Order = Util.FindChild(gameObject.transform.GetChild(0).gameObject, "Order");

        if (checkingCount == Order.transform.childCount)
        {
            Managers.UI.CreateSystemPopup("WarningPopup", "정답입니다.", UIManager.NoticeType.Info);

            if (Managers.Scenario.CurrentScenarioInfo.Action == "MPX_Clothing")
            {
                checkingCount = 0;
                fourProtects.Use(Managers.Object.MyPlayer);
                Managers.Scenario.MyAction = "MPX_Clothing";
                Destroy(gameObject.transform.GetChild(0).gameObject);
               Managers.Quiz.MPX_Clothing_Panel_opencheck = false;
            }
                
            else if (Managers.Scenario.CurrentScenarioInfo.Action == "MPX_LayOff")
            {
                checkingCount = 0;
                fourProtects.UnUse();
                Managers.Scenario.MyAction = "MPX_LayOff";
                Managers.Quiz.MPX_Clothing_Panel_opencheck = false;
                Destroy(gameObject);
            }
        }
        else
        {
            checkingCount = 0;
            Managers.UI.CreateSystemPopup("WarningPopup", "틀렸습니다.", UIManager.NoticeType.Warning);
            child = Managers.UI.CreateUI(Managers.Scenario.CurrentScenarioInfo.Action,gameObject.transform);
            Destroy(gameObject.transform.GetChild(0).gameObject);
            GameObject go = Util.FindChild(child, "Order_view");
            RandomSlot(go);
        }
    }

    public void RandomSlot(GameObject go = null)
    {
        if (go != null)
            Order_view = go;

        if(Order_view == null)
            Order_view = Util.FindChild(gameObject.transform.GetChild(0).gameObject, "Order_view");

        if(list !=null)
            list.Clear();

        for(int i = 0; i < Order_view.transform.childCount; i++)
        {
            list.Add(Order_view.transform.GetChild(i).GetComponent<RectTransform>().anchoredPosition);
        }

        ShuffleList(list);

        for(int i =0; i< Order_view.transform.childCount;i++)
        {
            Order_view.transform.GetChild(i).GetComponent<RectTransform>().anchoredPosition = list[i];
            Order_view.transform.GetChild(i).GetComponent<MPX_Clothing_Slot>().originalPosition = list[i];
        }
    }

    /// <summary>
    /// Fisher-Yates 알고리즘
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    void ShuffleList<T>(List<T>  list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int randomIndex = Random.Range(0, list.Count); // UnityEngine.Random 사용
            T temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
}
