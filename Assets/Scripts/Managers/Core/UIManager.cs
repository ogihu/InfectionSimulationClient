using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager
{
    public enum CanvasType
    {
        Overlay,
        World
    }

    public enum PopupType
    {
        AutoDestroy,
        ManualDestroy
    }

    public Dictionary<string, Stack<GameObject>> UICache { get; set; } = new Dictionary<string, Stack<GameObject>>();
    public Stack<GameObject> PopupStack { get; set; } = new Stack<GameObject>();
    public GameObject ScenarioPopup { get; set; }
    Dictionary<Transform, GameObject> BubbleCache { get; set; } = new Dictionary<Transform, GameObject>();
    Canvas overlayCanvas;
    Canvas worldCanvas;

    /// <summary>
    /// Resources/Prefabs/UI 폴더 산하에 있는 UI를 생성 및 리턴
    /// </summary>
    /// <param name="name">생성하려는 UI 오브젝트의 이름</param>
    /// <param name="canvasType">UI를 생성하려는 Canvas의 타입</param>
    /// <returns>GameObject</returns>
    public GameObject CreateUI(string name, CanvasType canvasType = CanvasType.Overlay)
    {
        if (name == null)
            Debug.LogError("Can't find null key, check the key input");

        GameObject go;
        Canvas canvas;

        if(canvasType == CanvasType.Overlay)
        {
            if (overlayCanvas == null)
                overlayCanvas = GameObject.Find("Canvas").GetComponent<Canvas>();
            canvas = overlayCanvas;
        }
        else
        {
            if (worldCanvas == null)
                worldCanvas = GameObject.Find("WorldCanvas").GetComponent<Canvas>();
            canvas = worldCanvas;

            if (worldCanvas.worldCamera == null)
                worldCanvas.worldCamera = Camera.main;
        }

        go = CreateUI(name, canvas.transform);

        return go;
    }

    public GameObject CreateUI(string name, Transform parent)
    {
        GameObject go;

        if (UICache.ContainsKey(name))
        {
            Stack<GameObject> stack = UICache[name];

            if (stack.Count > 0)
            {
                go = stack.Pop();
                go.transform.parent = parent;
            }
            else
                go = Managers.Resource.Instantiate($"UI/{name}", parent);

            go.SetActive(true);

            return go;
        }

        go = Managers.Resource.Instantiate($"UI/{name}", parent);
        UICache.Add(name, new Stack<GameObject>());
        go.SetActive(true);

        return go;
    }

    /// <summary>
    /// 화면 상단에 팝업 안내를 띄움
    /// </summary>
    /// <param name="notice">팝업에 표시하고 싶은 내용 입력</param>
    /// <returns>GameObject</returns>
    public GameObject CreateScenarioPopup(string notice, PopupType type = PopupType.AutoDestroy)
    {
        GameObject popup = CreateUI("PopupNotice");
        popup.GetComponentInChildren<TMP_Text>().text = notice;

        if (type == PopupType.AutoDestroy)
            Managers.Instance.StartCoroutine(DestroyAfter(popup, 3.0f));
        else
            ScenarioPopup = popup;

        return popup;
    }

    /// <summary>
    /// 오브젝트 상단에 말풍선을 띄움
    /// </summary>
    /// <param name="targetObject">말풍선을 띄우고 싶은 오브젝트</param>
    /// <param name="chat">말풍선에 표현하고 싶은 텍스트</param>
    /// <returns>GameObject</returns>
    public GameObject CreateChatBubble(Transform targetObject, string chat)
    {
        GameObject go;

        if (BubbleCache.ContainsKey(targetObject))
            go = BubbleCache[targetObject];
        else
        {
            go = CreateUI("Chat", CanvasType.World);
            BubbleCache.Add(targetObject, go);
        }

        go.GetComponent<PanelChatUI>().Init(targetObject, chat);

        return go;
    }

    public IEnumerator DestroyAfter(GameObject go, float time)
    {
        yield return new WaitForSeconds(time);
        DestroyUI(go);
    }

    public IEnumerator InvisibleAfter(GameObject go, float time)
    {
        yield return new WaitForSeconds(time);
        go.SetActive(false);
    }

    public void DestroyUI(GameObject go)
    {
        if (UICache.ContainsKey(go.name))
        {
            Stack<GameObject> stack = UICache[go.name];
            stack.Push(go);
            go.SetActive(false);
        }
        else
            UnityEngine.Object.Destroy(go);
    }

    public bool ExitPopup()
    {
        if (PopupStack.Count == 0)
            return false;

        GameObject go = PopupStack.Pop();
        DestroyUI(go);
        return true;
    }

    public void ClearChat()
    {
        //foreach(var chat in ActiveChat)
        //{
        //    DestroyUI(chat);
        //}
        //ActiveChat.Clear();
    }

    public void Clear()
    {
        UICache.Clear();
    }
}
