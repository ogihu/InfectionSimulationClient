using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackLoginButton : ButtonUI
{
    protected override void OnClicked()
    {
        base.OnClicked();

        C_LeaveLobby leavePacket = new C_LeaveLobby();
        leavePacket.TargetScene = Scene.LoginScene;
        Managers.Network.Send(leavePacket);

        Managers.Scene.LoadScene(Define.Scene.Login);
    }
}