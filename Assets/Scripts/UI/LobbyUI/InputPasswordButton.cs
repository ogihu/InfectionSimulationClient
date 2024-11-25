using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputPasswordButton : ButtonUI
{
    InputRoomPassword _parent;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void OnClicked()
    {
        base.OnClicked();

        _parent = Util.FindParentByName(gameObject, "InputRoomPassword").GetComponent<InputRoomPassword>();
        _parent.JoinRoom();
    }
}
