using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class PanelChatUI : FloatingUI
{
    public List<string> MessageBuffer = new List<string>();
    TMP_Text _message;
    TMP_Text _pageText;

    int _pageIndex;
    public bool _isVisible;

    public GameObject chatUI; // Chat UI 오브젝트
    public GameObject closeChatUI; // CloseChat UI 오브젝트

    public override void Init(Transform target, float x = 0, float y = 1.0f, float z = 0, bool isStatic = false)
    {
        _target = target;
        _isStatic = isStatic;
        _width = x;
        _height = y;
        _length = z;

        transform.position = new Vector3(target.position.x + _width, 1.6f, target.position.z + _length);

        if (_canvas == null)
        {
            _canvas = GetComponent<Canvas>();
        }

        chatUI = Util.FindChildByName(gameObject, "Chat");
        closeChatUI = Util.FindChildByName(gameObject, "CloseChat");

        //chatUI.transform.position = new Vector3(2.5f, 3.1f, 0.02f);
        //closeChatUI.transform.position = new Vector3(transform.position.x , 2.1f, -0.45f);

        if (chatUI == null || closeChatUI == null)
        {
            Debug.LogError("Chat 또는 CloseChat UI를 찾을 수 없습니다.");
        }

        _message = Util.FindChildByName(chatUI, "Message").GetComponent<TMP_Text>();
        _pageText = Util.FindChildByName(chatUI, "Page/Pages").GetComponent<TMP_Text>();

        if (_message == null || _pageText == null)
        {
            Debug.LogError("Message 또는 Page/Pages 컴포넌트를 찾을 수 없습니다.");
        }

        _isVisible = true;
        chatUI.SetActive(_isVisible);
        closeChatUI.SetActive(!_isVisible);
    }

    public override void ChasingTarget()
    {
        if (_target == null)
        {
            Debug.LogWarning("There is no target to chase, Check the target of this object");
            return;
        }

        if (_canvas == null)
        {
            Debug.LogWarning("There is no Canvas attached to this object, Check the Canvas component");
            return;
        }

        Vector3 targetPosition = new Vector3(_target.position.x + _width, 1.6f, _target.position.z + _length);

        if (GetComponent<RectTransform>().position != targetPosition)
        {
            GetComponent<RectTransform>().position = targetPosition;
        }

        if (_canvas != null)
        {
            if (Camera.main == null)
                return;

            int distance = (int)(Camera.main.transform.position - transform.position).magnitude;

            if (distance == 0)
                _canvas.sortingOrder = 0;
            else
                _canvas.sortingOrder = (1 / distance) * 100;
        }
        else
        {
            Debug.LogWarning("Canvas 컴포넌트를 찾을 수 없습니다. sortingOrder를 설정할 수 없습니다.");
        }
    }

    public override void ChangeMessage(string chat)
    {
        SetBuffer(chat);
        _pageIndex = 0;
        ChangeDisplay();
    }

    void SetBuffer(string chat)
    {
        MessageBuffer.Clear();

        if (string.IsNullOrEmpty(chat))
            return;

        string[] chatSplit = chat.Split(new[] { "\n" }, System.StringSplitOptions.None);

        foreach (var message in chatSplit)
        {
            if (!string.IsNullOrEmpty(message))
            {
                MessageBuffer.AddRange(SplitStringByLength(message, 60));
            }
        }
    }

    void ChangeDisplay()
    {
        if (MessageBuffer.Count == 0)
        {
            if (_message != null) _message.text = string.Empty;
            if (_pageText != null) _pageText.text = "0 / 0";
            return;
        }

        if (_message != null) _message.text = MessageBuffer[_pageIndex];
        if (_pageText != null) _pageText.text = $"{_pageIndex + 1} / {MessageBuffer.Count}";
    }

    public void NextPage()
    {
        if (_pageIndex >= MessageBuffer.Count - 1)
            return;

        _pageIndex++;
        ChangeDisplay();
    }

    public void PrevPage()
    {
        if (_pageIndex <= 0)
            return;

        _pageIndex--;
        ChangeDisplay();
    }

    public void OpenOrCloseBubble()
    {
        _isVisible = !_isVisible;

        chatUI.SetActive(_isVisible);
        closeChatUI.SetActive(!_isVisible);
    }

    List<string> SplitStringByLength(string str, int length)
    {
        List<string> result = new List<string>();
        int strLength = str.Length;

        for (int i = 0; i < strLength; i += length)
        {
            if (i + length > strLength)
            {
                result.Add(str.Substring(i));
            }
            else
            {
                result.Add(str.Substring(i, length));
            }
        }

        return result;
    }
}
