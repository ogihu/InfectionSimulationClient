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

    public enum NoticeType
    {
        None,
        Warning,
        Info
    }

    public Stack<GameObject> ContentPopups { get; set; } = new Stack<GameObject>();
    public Dictionary<string, SystemPopup> SystemPopups { get; set; } = new Dictionary<string, SystemPopup>();
    Dictionary<Transform, GameObject> BubbleCache { get; set; } = new Dictionary<Transform, GameObject>();
    public GameObject SettingUI { get; set; }

    Canvas _overlayCanvas;
    public Canvas OverlayCanvas
    {
        get
        {
            if(_overlayCanvas == null)
            {
                _overlayCanvas = GameObject.Find("Canvas").GetComponent<Canvas>();
            }

            return _overlayCanvas;
        }
    }

    Canvas _worldCanvas;
    public Canvas WorldCanvas
    {
        get
        {
            if(_worldCanvas == null)
                _worldCanvas = GameObject.Find("WorldCanvas").GetComponent<Canvas>();

            return _worldCanvas;
        }
    }

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

        if (canvasType == CanvasType.Overlay)
        {
            canvas = OverlayCanvas;
        }
        else
        {
            canvas = WorldCanvas;

            if (WorldCanvas.worldCamera == null)
                WorldCanvas.worldCamera = Camera.main;
        }

        go = CreateUI(name, canvas.transform);

        return go;
    }

    public GameObject CreateUI(string name, Transform parent)
    {
        GameObject go = Managers.Resource.Load<GameObject>($"Prefabs/UI/{name}");
        go = CreateUI(go, parent);

        return go;
    }

    public GameObject CreateUI(GameObject obj, Transform parent)
    {
        GameObject go = Managers.Resource.Instantiate(obj, parent);
        if (go.GetComponent<Poolable>())
            go.GetComponent<RectTransform>().anchoredPosition3D = Vector3.zero;
        return go;
    }

    public void OpenOrCloseSetting()
    {
        if(SettingUI == null)
        {
            SettingUI = CreateUI("Setting");
            Managers.Object.MyPlayer.State = Google.Protobuf.Protocol.CreatureState.Setting;
        }
        else
        {
            DestroyUI(SettingUI);
            SettingUI = null;
            Managers.Object.MyPlayer.State = Google.Protobuf.Protocol.CreatureState.Idle;
        }
    }

    /// <summary>
    /// 화면 상단에 팝업 안내를 띄움
    /// </summary>
    /// <param name="notice">팝업에 표시하고 싶은 내용 입력</param>
    /// <returns>GameObject</returns>
    public GameObject CreateSystemPopup(string name, string notice, NoticeType noticeType, PopupType type = PopupType.AutoDestroy, float time = 3.0f)
    {
        //TODO : 인자에 NoticeType 추가하여 점수에 참조할 수 있도록
        SystemPopup popup;

        if (SystemPopups.ContainsKey(name))
            popup = SystemPopups[name];
        else
        {
            GameObject go = CreateUI(name);
            popup = go.GetComponent<SystemPopup>();
            SystemPopups.Add(name, popup);
        }

        switch (noticeType)
        {
            case NoticeType.None:
                popup.ChangeText(notice);
                break;
            case NoticeType.Info:
                popup.ChangeText($"<color=#0000ff>{notice}</color>");
                break;
            case NoticeType.Warning:
                popup.ChangeText($"<color=#ff0000>{notice}</color>");

                if (Managers.Scenario.CurrentScenarioInfo != null)
                {
                    Managers.Scenario.Score -= 1;
                    Debug.Log($"Score : {Managers.Scenario.Score}");
                }

                break;
        }

        if (type == PopupType.AutoDestroy)
            popup.AutoDestroy(time);

        return popup.gameObject;
    }

    /// <summary>
    /// 오브젝트 상단에 말풍선을 띄움
    /// </summary>
    /// <param name="targetObject">말풍선을 띄우고 싶은 오브젝트</param>
    /// <param name="chat">말풍선에 표현하고 싶은 텍스트</param>
    /// <returns>GameObject</returns>
    public GameObject CreateChatBubble(Transform targetObject)
    {
        GameObject go = CreateUI("PanelChat", CanvasType.World);
        PanelChatUI panelChatUI = go.GetComponent<PanelChatUI>();
        if (panelChatUI != null)
        {
            panelChatUI.Init(targetObject, x : 0.0f,y : 1.65f);     // 초기화
        }
        else
        {
            Debug.LogError("PanelChatUI 컴포넌트를 찾을 수 없습니다.");
        }

        BubbleCache.Add(targetObject, go);
        go.SetActive(false);

        return go;
    }

    public void DestroyBubble(Transform owner)
    {
        if (!BubbleCache.ContainsKey(owner))
            return;

        DestroyUI(BubbleCache[owner]);
    }

    public void ChangeChatBubble(Transform host, string message, bool playTTS = true)
    {
        if (!BubbleCache[host].activeSelf)
            BubbleCache[host].SetActive(true);

        BubbleCache[host].GetComponent<FloatingUI>().ChangeMessage(message);

        //추가
        BaseController bc = host.GetComponent<BaseController>();

        //TODO : 나중에 주석 풀어야 됨
        if (bc != null && playTTS)
            Managers.TTS.Speaking(host, message);
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

    public IEnumerator BubbleInvisibleAfter(Transform host, float time)
    {
        yield return new WaitForSeconds(time);
        InvisibleBubble(host);
    }

    public void InvisibleBubble(Transform host)
    {
        if (BubbleCache[host] != null)
            BubbleCache[host].SetActive(false);
    }

    public void DestroyUI(GameObject go)
    {
        Managers.Resource.Destroy(go);
    }

    public bool ExitPopup()
    {
        if (ContentPopups.Count == 0)
            return false;

        GameObject go = ContentPopups.Pop();
        DestroyUI(go);
        return true;
    }

    public void Clear()
    {
        ContentPopups.Clear();
        SystemPopups.Clear();
        BubbleCache.Clear();
        SettingUI = null;
        _overlayCanvas = null;
        _worldCanvas = null;
    }
}
