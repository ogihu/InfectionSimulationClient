using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitRoomButton : ButtonUI
{
    protected override void OnClicked()
    {
        base.OnClicked();

        C_LeaveRoom leavePacket = new C_LeaveRoom();
        leavePacket.TargetScene = Scene.LobbyScene;
        Managers.Network.Send(leavePacket);

        Managers.Scene.LoadScene(Define.Scene.Lobby);
    }
}
