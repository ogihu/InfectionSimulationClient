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
        if (string.IsNullOrEmpty(chat))
            return;

        MessageBuffer.Clear();
        string[] chatSplit = chat.Split('.');

        foreach(var message in chatSplit)
        {
            if(!string.IsNullOrEmpty(message))
                MessageBuffer.Add(message);
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
}
