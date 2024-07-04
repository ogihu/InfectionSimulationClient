using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonPhoneFuncSelect : ButtonUI
{
    protected override void OnClicked()
    {
        base.OnClicked();
        Managers.Phone.Device.FuncSelect(gameObject.name);
    }
}
