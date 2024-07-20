using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonInventory : ButtonUI
{
    Action<Item> _action;

    protected override void OnClicked()
    {
        base.OnClicked();
        _action.Invoke(Managers.Inventory.SelectedItem);
    }

    public void SetEvent(Action<Item> action)
    {
        _action = action;
    }
}
