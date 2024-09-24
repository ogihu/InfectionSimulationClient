using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonItemSlot : ButtonUI
{
    ItemInfo _item;
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
        if(Managers.Item.IsCombining == false)
        {
            Managers.Item.SelectedItem = _item;
            Managers.Item.Inventory.ChangeItemText(_item.ItemData.Name);
        }
        else
        {
            if (Managers.Item.CombineItems.Contains(_item))
            {
                Managers.Item.CombineItems.Remove(_item);
                Managers.Item.Inventory.UpdateCombineItemsText();
            }
            else
            {
                if(Managers.Item.CombineItems.Count < 2)
                {
                    Managers.Item.CombineItems.Add(_item);
                    Managers.Item.Inventory.UpdateCombineItemsText();
                }
            }
        }
        return;
    }

    public void SetItem(ItemInfo item)
    {
        _item = item;
        _icon.sprite = _item.ItemData.Icon;
        _equiped.SetActive(_item.Using);
    }
}
