using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using System.Web;
using TMPro;
using UnityEngine;

public class LoginUI : MonoBehaviour
{
    TMP_InputField inputId;
    TMP_InputField inputPw;

    private void Awake()
    {
        inputId = Util.FindChildByName(gameObject, "InputId").GetComponent<TMP_InputField>();
        inputPw = Util.FindChildByName(gameObject, "InputPassword").GetComponent<TMP_InputField>();
    }

    bool CheckEmptyField()
    {
        if (string.IsNullOrEmpty(inputId.text) || string.IsNullOrEmpty(inputPw.text))
        {
            return false;
        }
        else
            return true;
    }

    public void SendAllInfo()
    {
        if (!CheckEmptyField())
        {
            GameObject go = Managers.UI.CreateUI("WarningUI");
            go.GetComponent<WarningUI>().SetText("입력되지 않은 정보가 있습니다.");
            return;
        }

        Managers.Network.WaitingUI = Managers.UI.CreateUI("WaitingUI");

        C_Login loginPacket = new C_Login();
        loginPacket.AccountId = inputId.text;
        loginPacket.AccountPw = inputPw.text;

        Managers.Network.Send(loginPacket);
    }
}
