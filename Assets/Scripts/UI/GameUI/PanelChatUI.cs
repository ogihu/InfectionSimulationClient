using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PanelChatUI : FloatingUI
{
    public List<string> MessageBuffer = new List<string>();
    TMP_Text _message;
    TMP_Text _pageText;

    int _pageIndex;

    private void Awake()
    {
        _message = Util.FindChildByName(gameObject, "Message").GetComponent<TMP_Text>();
        _pageText = Util.FindChildByName(gameObject, "Page/Pages").GetComponent<TMP_Text>();
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

        string[] chatSplit = chat.Split(".\n");

        foreach(var message in chatSplit)
        {
            if (!string.IsNullOrEmpty(message))
            {
                if (message.Length <= 54)
                {
                    MessageBuffer.Add(message);
                }
                else
                {
                    int startIndex = 0;
                    while (startIndex < message.Length)
                    {
                        int length = Math.Min(54, message.Length - startIndex);
                        MessageBuffer.Add(message.Substring(startIndex, length));
                        startIndex += length;
                    }
                }
            }
        }
    }

    void ChangeDisplay()
    {
        _message.text = MessageBuffer[_pageIndex];
        _pageText.text = $"{_pageIndex + 1} / {MessageBuffer.Count}";
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

    public void CloseBubble()
    {
        Managers.UI.DestroyUI(this.gameObject);
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
