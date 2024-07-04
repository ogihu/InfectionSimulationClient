using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonPhoneFinishCall : ButtonUI
{
    protected override void OnClicked()
    {
        base.OnClicked();
        Managers.Phone.Device.FinishCall();
    }
}
