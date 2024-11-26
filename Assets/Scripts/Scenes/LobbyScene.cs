using Google.Protobuf.Protocol;
using UnityEngine;

public class LobbyScene : BaseScene
{
    public LobbyUI LobbyUI { get; set; }

    protected override void Init()
    {
        base.Init();

        SceneType = Define.Scene.Lobby;

        Application.runInBackground = true;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        LobbyUI = GameObject.Find("LobbyUI").GetComponent<LobbyUI>();

        C_RequestRoomList requestPacket = new C_RequestRoomList();
        Managers.Network.Send(requestPacket);
    }

    public override void Clear()
    {
        LobbyUI = null;
    }
}
