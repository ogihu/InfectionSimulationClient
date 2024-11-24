using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MakeRoomButton : ButtonUI
{
    MakingRoomUI _makingRoomUI;

    protected override void OnClicked()
    {
        base.OnClicked();

        if(_makingRoomUI == null)
            _makingRoomUI = Util.FindParentByName(gameObject, "MakingRoomUI").GetComponent<MakingRoomUI>();

        _makingRoomUI.MakeRoom();
    }
}
