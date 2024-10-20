using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RegistButton : ButtonUI
{
    protected override void OnClicked()
    {
        base.OnClicked();

        transform.parent.GetComponent<RegistUI>().SendAllInfo();
    }
}
