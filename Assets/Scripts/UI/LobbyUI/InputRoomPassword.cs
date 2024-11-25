using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InputRoomPassword : MonoBehaviour
{
    RoomInfo RoomInfo;
    TMP_InputField password;

    public void Init(RoomInfo info)
    {
        RoomInfo = info;
        password = Util.FindChildByName(gameObject, "InputPassword").GetComponent<TMP_InputField>();
    }

    public void JoinRoom()
    {
        if(string.IsNullOrEmpty(password.text))
        {
            GameObject warningUI = Managers.UI.CreateUI("WarningUI");
            warningUI.GetComponent<WarningUI>().SetText("비밀번호를 입력하세요.");
            return;
        }

        C_EnterRoom packet = new C_EnterRoom();
        packet.Title = RoomInfo.Title;
        packet.Password = password.text;
        Managers.Network.Send(packet);
        Managers.Network.WaitingUI = Managers.UI.CreateUI("WaitingUI");
    }
}
