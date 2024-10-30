using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenRegistUIButton : ButtonUI
{
    protected override void OnClicked()
    {
        base.OnClicked();
        Managers.UI.CreateUI("RegistUI");
    }
}
