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
        base.Init(target,x, y, z ,isStatic);

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

        string[] chatSplit = chat.Split(new[] { ".\n" }, System.StringSplitOptions.None);

        foreach (var message in chatSplit)
        {
            if (!string.IsNullOrEmpty(message))
            {
                MessageBuffer.AddRange(SplitStringByLength(message, 54));
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
