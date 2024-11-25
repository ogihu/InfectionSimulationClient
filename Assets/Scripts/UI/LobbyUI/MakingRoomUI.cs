using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MakingRoomUI : MonoBehaviour
{
    TMP_Dropdown _diseaseDropdown;
    TMP_InputField _title;
    TMP_InputField _password;

    private void Awake()
    {
        _diseaseDropdown = Util.FindChildByName(gameObject, "DiseaseDropdown").GetComponent<TMP_Dropdown>();
        _title = Util.FindChildByName(gameObject, "InputTitle").GetComponent<TMP_InputField>();
        _password = Util.FindChildByName(gameObject, "InputPassword").GetComponent<TMP_InputField>();
    }

    public void MakeRoom()
    {
        if (string.IsNullOrEmpty(_title.text))
        {
            GameObject warningUI = Managers.UI.CreateUI("WarningUI");
            warningUI.GetComponent<WarningUI>().SetText("방 제목이 입력되지 않았습니다.");
            return;
        }

        if(_diseaseDropdown.options[_diseaseDropdown.value].text == "Select Disease")
        {
            GameObject warningUI = Managers.UI.CreateUI("WarningUI");
            warningUI.GetComponent<WarningUI>().SetText("시나리오를 선택하세요.");
            return;
        }

        Managers.Network.WaitingUI = Managers.UI.CreateUI("WaitingUI");

        C_MakeRoom roomPacket = new C_MakeRoom();
        roomPacket.Title = _title.text;
        roomPacket.Disease = _diseaseDropdown.options[_diseaseDropdown.value].text;
        roomPacket.Password = _password.text;
        Managers.Network.Send(roomPacket);
    }
}
