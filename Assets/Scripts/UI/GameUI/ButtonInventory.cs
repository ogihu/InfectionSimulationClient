using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonInventory : ButtonUI
{
    Action<ItemInfo> _myAction;

    protected override void OnClicked()
    {
        base.OnClicked();
        _myAction.Invoke(Managers.Item.SelectedItem);
    }

    public void SetEvent(Action<ItemInfo> action)
    {
        _myAction = action;
    }
}
