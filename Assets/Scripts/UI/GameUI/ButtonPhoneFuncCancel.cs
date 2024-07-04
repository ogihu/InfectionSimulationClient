using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonPhoneFuncCancel : ButtonUI
{
    protected override void OnClicked()
    {
        base.OnClicked();
        Managers.Phone.Device.FuncCancel();
    }
}
