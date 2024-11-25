using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewRoomButton : ButtonUI
{
    protected override void OnClicked()
    {
        base.OnClicked();

        Managers.UI.CreateUI("Lobby/MakingRoomUI");
    }
}
