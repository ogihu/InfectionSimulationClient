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

    public Dictionary<string, Stack<GameObject>> UICache { get; set; } = new Dictionary<string, Stack<GameObject>>();
    Canvas overlayCanvas;
    Canvas worldCanvas;

    /// <summary>
    /// Resources/Prefabs/UI 폴더 산하에 있는 UI를 생성
    /// 해당 UI의 name을 매개변수로 넘겨주면 됨
    /// 생성된 UI를 반환해주기 때문에 GameObject로 받아 해당 UI에 필요한 작업 수행 가능
    /// </summary>
    /// <param name="name"></param>
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

        if (UICache.ContainsKey(name))
        {
            Stack<GameObject> stack = UICache[name];
            
            if(stack.Count > 0)
            {
                go = stack.Pop();
                go.transform.parent = canvas.transform;
            }
            else
                go = Managers.Resource.Instantiate($"UI/{name}", canvas.transform);

            go.SetActive(true);

            return go;
        }

        go = Managers.Resource.Instantiate($"UI/{name}", canvas.transform);
        UICache.Add(name, new Stack<GameObject>());
        go.SetActive(true);

        return go;
    }

    public GameObject CreatePopup(string notice)
    {
        GameObject popup = CreateUI("PopupNotice");
        popup.GetComponentInChildren<TMP_Text>().text = notice;
        Managers.Instance.StartCoroutine(DestroyAfter(popup, 3.0f));
        return popup;
    }

    public GameObject CreateChatUI(Transform targetObject, string chat)
    {
        GameObject go = CreateUI("Chat", CanvasType.World);
        go.GetComponent<PanelChatUI>().Init(targetObject, chat);
        return go;
    }

    public void ClearChat()
    {
        Stack<GameObject> chatUIs = new Stack<GameObject>();

        if (UICache.TryGetValue("Chat", out chatUIs))
        {
            foreach(var chat in chatUIs)
            {
                Managers.Resource.Destroy(chat);
            }
        }
    }

    public IEnumerator DestroyAfter(GameObject go, float time)
    {
        yield return new WaitForSeconds(time);
        DestroyUI(go);
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

    public void Clear()
    {
        UICache.Clear();
    }
}
