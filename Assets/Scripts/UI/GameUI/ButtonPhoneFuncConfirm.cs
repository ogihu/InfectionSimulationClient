using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonPhoneFuncConfirm : ButtonUI
{
    protected override void OnClicked()
    {
        base.OnClicked();
        Managers.Phone.Device.FuncConfirm();
    }
}
