using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonExitUI : ButtonUI
{
    protected override void OnClicked()
    {
        base.OnClicked();
        Managers.UI.DestroyUI(transform.parent.gameObject);
    }
}
