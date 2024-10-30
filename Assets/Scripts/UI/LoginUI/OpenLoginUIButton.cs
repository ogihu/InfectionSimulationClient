using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenLoginUIButton : ButtonUI
{
    protected override void OnClicked()
    {
        base.OnClicked();
        Managers.UI.CreateUI("LoginUI");
    }
}
