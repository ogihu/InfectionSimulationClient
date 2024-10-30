using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Google.Protobuf.Protocol;

public class ButtonLoginUI : ButtonUI
{
    protected override void Awake()
    {
        base.Awake();
    }

    protected override void OnClicked()
    {
        base.OnClicked();

        transform.parent.GetComponent<LoginUI>().SendAllInfo();
    }
}
