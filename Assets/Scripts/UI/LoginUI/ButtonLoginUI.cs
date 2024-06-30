using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Google.Protobuf.Protocol;

public class ButtonLoginUI : ButtonUI
{
    protected override void OnClicked()
    {
        base.OnClicked();
        GameObject go = Managers.Resource.FindObject("InputName");
        string name = go.GetComponent<TMP_InputField>().text;

        go = Managers.Resource.FindObject("InputId");
        string id = go.GetComponent<TMP_InputField>().text;

        go = Managers.Resource.FindObject("SelectPosition");
        string position = go.GetComponent<TMP_Dropdown>().options[go.GetComponent<TMP_Dropdown>().value].text;

        C_Login loginPacket = new C_Login();
        loginPacket.UserInfo = new UserInfo();
        loginPacket.UserInfo.Name = name;
        loginPacket.UserInfo.Id = id;
        loginPacket.UserInfo.Position = position;
        Managers.Network.Send(loginPacket);
    }
}
