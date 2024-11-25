using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomButton : ButtonUI
{
    LobbyUI _lobbyUI;
    public RoomInfo RoomInfo { get; set; }

    TMP_Text _title;
    TMP_Text _maker;
    TMP_Text _disease;
    TMP_Text _members;
    Image _secret;

    Sprite _lock;
    Sprite _unlock;

    public void Init(RoomInfo info)
    {
        _lobbyUI = Util.FindParentByName(gameObject, "LobbyUI").GetComponent<LobbyUI>();
        RoomInfo = info;

        _title = Util.FindChildByName(gameObject, "Title").GetComponent<TMP_Text>();
        _maker = Util.FindChildByName(gameObject, "Player").GetComponent<TMP_Text>();
        _disease = Util.FindChildByName(gameObject, "Disease").GetComponent<TMP_Text>();
        _members = Util.FindChildByName(gameObject, "Members").GetComponent<TMP_Text>();
        _secret = Util.FindChildByName(gameObject, "Secret").GetComponentInChildren<Image>();
        _lock = Resources.Load<Sprite>("Sprites/Lobby/Lock");
        _unlock = Resources.Load<Sprite>("Sprites/Lobby/UnLock");

        _title.text = info.Title;
        _maker.text = info.Maker;
        _disease.text = info.Disease;
        _members.text = info.CurMembers.ToString();

        if (string.IsNullOrEmpty(info.Password))
            _secret.sprite = _unlock;
        else
            _secret.sprite = _lock;
    }

    protected override void OnClicked()
    {
        base.OnClicked();

        _lobbyUI.UpdateSelectedRoom(RoomInfo);
    }
}
