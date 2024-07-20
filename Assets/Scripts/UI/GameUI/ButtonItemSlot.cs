using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonItemSlot : ButtonUI
{
    Item _item;
    Image _icon;
    GameObject _equiped;

    protected override void Awake()
    {
        base.Awake();
        _icon = Util.FindChildByName(gameObject, "ItemIcon").GetComponent<Image>();
        _equiped = Util.FindChildByName(gameObject, "Equiped");
    }

    protected override void OnClicked()
    {
        base.OnClicked();
        Managers.Inventory.SelectedItem = _item;
        Managers.Inventory.Inventory.ChangeItemText(_item.ItemData.Name);
    }

    public void SetItem(Item item)
    {
        _item = item;
        _icon.sprite = _item.ItemData.Icon;
        _equiped.SetActive(_item.Equiped);
    }
}
