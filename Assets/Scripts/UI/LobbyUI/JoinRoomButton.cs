using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JoinRoomButton : ButtonUI
{
    LobbyUI _lobbyUI;

    protected override void Awake()
    {
        base.Awake();

        _lobbyUI = Util.FindParentByName(gameObject, "LobbyUI").GetComponent<LobbyUI>();
    }

    protected override void OnClicked()
    {
        base.OnClicked();

        _lobbyUI.JoinRoom();
    }
}
