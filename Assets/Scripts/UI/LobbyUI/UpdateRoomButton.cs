using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateRoomButton : ButtonUI
{
    Animator _animator;

    protected override void Awake()
    {
        base.Awake();

        _animator = GetComponent<Animator>();
    }

    protected override void OnClicked()
    {
        base.OnClicked();

        C_RequestRoomList requestPacket = new C_RequestRoomList();
        Managers.Network.Send(requestPacket);

        _animator.Play("RoomUpdateAnim");
    }
}
