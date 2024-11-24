using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyUI : MonoBehaviour
{
    Transform RoomListRoot;
    RoomInfo selectedRoom;

    void Awake()
    {
        RoomListRoot = Util.FindChildByName(gameObject, "Content").transform;
        selectedRoom = null;
    }

    public void UpdateRoomList(RoomInfo[] roomInfos)
    {
        for(int i = 0; i < RoomListRoot.childCount; i++)
        {
            Managers.UI.DestroyUI(RoomListRoot.GetChild(i).gameObject);
        }

        for(int i = 0; i < roomInfos.Length; i++)
        {
            RoomButton room = Managers.UI.CreateUI("Lobby/Room", RoomListRoot).GetComponent<RoomButton>();
            room.Init(roomInfos[i]);
        }
    }

    public void UpdateSelectedRoom(RoomInfo info)
    {
        selectedRoom = info;
    }

    public void JoinRoom()
    {
        if (selectedRoom == null)
            return;

        if (string.IsNullOrEmpty(selectedRoom.Password))
        {
            C_EnterRoom packet = new C_EnterRoom();
            packet.Title = selectedRoom.Title;
            packet.Password = selectedRoom.Password;
            Managers.Network.Send(packet);
            Managers.Network.WaitingUI = Managers.UI.CreateUI("WaitingUI");
        }
        else
        {
            InputRoomPassword passwordUI = Managers.UI.CreateUI("Lobby/InputRoomPassword").GetComponent<InputRoomPassword>();
            passwordUI.Init(selectedRoom);
        }
    }
}
