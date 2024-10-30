using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class BubbleManager
{
    public PanelChatUI SelectedChat { get; set; }
    Color _normalColor = Color.white;

    public void NextPage()
    {
        if (SelectedChat == null)
            return;

        SelectedChat.NextPage();
    }

    public void PrevPage()
    {
        if (SelectedChat == null)
            return;

        SelectedChat.PrevPage();
    }

    public void OpenOrCloseBubble()
    {
        if (SelectedChat == null)
            return;

        SelectedChat.OpenOrCloseBubble();
    }

    public void ChangeButtonColor()
    {
        if (SelectedChat == null)
            return;

        Button button = SelectedChat.GetComponentInChildren<Button>();
        if (button == null)
            return;

        ColorBlock cb = button.colors;
        cb.normalColor = button.colors.pressedColor;
        button.colors = cb;
        Managers.Instance.StartCoroutine(CoRestoreButton(0.1f));
    }

    IEnumerator CoRestoreButton(float time)
    {
        yield return new WaitForSeconds(time);
        if (SelectedChat == null)
            yield break;

        Button button = SelectedChat.GetComponentInChildren<Button>();
        if (button == null)
            yield break;

        ColorBlock cb = button.colors;
        cb.normalColor = _normalColor;
        button.colors = cb;
    }

    public void Clear()
    {
        SelectedChat = null;
    }
}
