using Google.Protobuf.Protocol;
using UnityEngine;

public class RoomScene : BaseScene
{
    RoomUI _roomUI;
    public RoomUI RoomUI
    {
        get
        {
            if(_roomUI == null)
                _roomUI = GameObject.Find("RoomUI").GetComponent<RoomUI>();

            if(_roomUI == null)
                _roomUI = Managers.UI.CreateUI("Room/RoomUI").GetComponent<RoomUI>();

            return _roomUI;
        }
    }

    protected override void Init()
    {
        base.Init();

        SceneType = Define.Scene.Room;

        Application.runInBackground = true;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        C_RequestRoomInfo requestPacket = new C_RequestRoomInfo();
        Managers.Network.Send(requestPacket);
    }

    public override void Clear()
    {
        base.Clear();
        _roomUI = null;
    }
}
