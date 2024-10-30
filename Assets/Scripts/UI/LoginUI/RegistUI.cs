using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RegistUI : MonoBehaviour
{
    TMP_InputField inputAccountId;
    TMP_InputField inputAccountPw;
    TMP_InputField inputPlayerName;
    TMP_InputField confirmPw;

    private void Awake()
    {
        inputAccountId = Util.FindChildByName(gameObject, "InputAccountId").GetComponent<TMP_InputField>();
        inputAccountPw = Util.FindChildByName(gameObject, "InputAccountPw").GetComponent<TMP_InputField>();
        inputPlayerName = Util.FindChildByName(gameObject, "InputPlayerName").GetComponent<TMP_InputField>();
        confirmPw = Util.FindChildByName(gameObject, "PwConfirm").GetComponent<TMP_InputField>();
    }

    /// <summary>
    /// RegistUI의 InputField 요소들 중 입력받지 않은 필드가 있을 경우 false, 모두 입력받은 경우 true 반환
    /// </summary>
    /// <returns></returns>
    public bool CheckEmptyField()
    {
        if(string.IsNullOrEmpty(inputAccountId.text) || string.IsNullOrEmpty(inputAccountPw.text) || string.IsNullOrEmpty(inputPlayerName.text) || string.IsNullOrEmpty(confirmPw.text))
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public void SendAllInfo()
    {
        if(!CheckEmptyField())
        {
            GameObject go = Managers.UI.CreateUI("WarningUI");
            go.GetComponent<WarningUI>().SetText("입력되지 않은 정보가 있습니다.");
            return;
        }

        if (inputAccountPw.text != confirmPw.text)
        {
            GameObject go = Managers.UI.CreateUI("WarningUI");
            go.GetComponent<WarningUI>().SetText("비밀번호가 일치하지 않습니다.");
            return;
        }

        Managers.Network.WaitingUI = Managers.UI.CreateUI("WaitingUI");

        C_RegistAccount registPacket = new C_RegistAccount();
        registPacket.AccountId = inputAccountId.text;
        registPacket.AccountPw = inputAccountPw.text;
        registPacket.PlayerName = inputPlayerName.text;

        Managers.Network.Send(registPacket);
    }
}
