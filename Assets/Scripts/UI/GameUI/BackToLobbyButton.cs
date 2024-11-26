using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackToLobbyButton : ButtonUI
{
    protected override void OnClicked()
    {
        base.OnClicked();

        C_ExitGame exitPacket = new C_ExitGame();
        Managers.Network.Send(exitPacket);

        Managers.Scene.LoadScene(Define.Scene.Lobby);
    }
}
