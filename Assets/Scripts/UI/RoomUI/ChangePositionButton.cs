using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ChangePositionButton : ButtonUI
{
    public TMP_Text _idText;
    public int Index {  get; set; }

    protected override void Awake()
    {
        base.Awake();
        _idText = GetComponentInChildren<TMP_Text>();
    }

    protected override void OnClicked()
    {
        if (_idText.text != "없음")
            return;

        base.OnClicked();

        C_ChangePosition changePacket = new C_ChangePosition();
        changePacket.Position = Index;
        Managers.Network.Send(changePacket);
    }

    public void SetText(string text)
    {
        _idText.text = text;
    }
}
