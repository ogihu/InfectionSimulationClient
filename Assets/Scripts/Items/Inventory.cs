using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
    Transform _content;
    
    TMP_Text _itemName;
    ButtonInventory _equipButton;
    ButtonInventory _unEquipButton;
    ButtonInventory _dropButton;

    List<ButtonItemSlot> _itemSlots = new List<ButtonItemSlot>();

    private void Awake()
    {
        _content = Util.FindChildByName(gameObject, "Content").transform;
        _itemName = Util.FindChildByName(gameObject, "ItemName").GetComponent<TMP_Text>();
        _equipButton = Util.FindChildByName(gameObject, "EquipButton").GetComponent<ButtonInventory>();
        _unEquipButton = Util.FindChildByName(gameObject, "UnEquipButton").GetComponent<ButtonInventory>();
        _dropButton = Util.FindChildByName(gameObject, "DropButton").GetComponent<ButtonInventory>();

        _equipButton.SetEvent(Managers.Inventory.EquipItem);
        _unEquipButton.SetEvent(Managers.Inventory.UnEquipItem);
        _dropButton.SetEvent(Managers.Inventory.DropItem);
    }

    public void UpdateItemList()
    {
        int i = 0;
        for (; i < Managers.Inventory.ItemList.Count; i++)
        {
            if (i < _itemSlots.Count)
                _itemSlots[i].SetItem(Managers.Inventory.ItemList[i]);
            else
            {
                ButtonItemSlot itemSlot = Managers.UI.CreateUI("ItemSlot", _content).GetComponent<ButtonItemSlot>();
                itemSlot.SetItem(Managers.Inventory.ItemList[i]);
                _itemSlots.Add(itemSlot);
            }
        }

        for (; i < _itemSlots.Count; i++)
        {
            Managers.UI.DestroyUI(_itemSlots[i].gameObject);
        }

        Debug.Log($"아이템 슬롯 업데이트 - 보유한 아이템 {Managers.Inventory.ItemList.Count}개, 슬롯 {_itemSlots.Count}개");
    }


    public void ChangeItemText(string name)
    {
        switch (name)
        {
            case "Mask":
                name = "마스크";
                break;
            case "ProtectiveGear":
                name = "4종 보호구";
                break;
        }
        _itemName.text = name;
    }

    private void OnEnable()
    {
        _itemName.text = "아이템 이름";
        UpdateItemList();
    }
}
