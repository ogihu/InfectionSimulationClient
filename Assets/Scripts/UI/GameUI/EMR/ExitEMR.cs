using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitEMR : ButtonExitUI
{
    protected override void OnClicked()
    {
        base.OnClicked();
        Managers.EMR.CloseEMR();
    }
}
