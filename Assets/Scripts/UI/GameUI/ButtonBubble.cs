using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonBubble : ButtonUI
{
    protected override void OnClicked()
    {
        base.OnClicked();
        Managers.Bubble.SelectedChat = GetComponentInParent<PanelChatUI>();
    }
}
