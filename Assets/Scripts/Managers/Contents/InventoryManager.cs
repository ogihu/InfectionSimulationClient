using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Item
{
    public ItemData ItemData { get; set; }
    public GameObject Object { get; set; }
    public bool Equiped { get; set; }
}

public class InventoryManager
{
    public List<Item> ItemList = new List<Item>();
    public Inventory Inventory { get; set; }
    public Item SelectedItem { get; set; }

    public void GetItem(ItemData itemData)
    {
        Item item = new Item();
        item.ItemData = itemData;
        item.Equiped = false;

        ItemList.Add(item);
        Debug.Log($"아이템 획득 - 현재 보유한 아이템 {ItemList.Count}개");
    }

    public void EquipItem(Item item)
    {
        if(item == null)
        {
            Debug.Log("There is no selected item");
            return;
        }

        if (item.Equiped == true || item.Object != null)
            return;

        //기존에 착용한 장비는 해제하고 새 장비 착용
        ItemList.ForEach(item => UnEquipItem(item));
        item.Object = Managers.Resource.Instantiate(item.ItemData.Prefab);
        item.Equiped = true;

        Equipment equipment = item.Object.GetComponent<Equipment>();

        if (equipment == null)
        {
            Debug.Log("Can't find the component : Equipment");
            return;
        }

        C_Equip equipPacket = new C_Equip();
        equipPacket.ItemName = item.ItemData.Name;
        Managers.Network.Send(equipPacket);

        equipment.Equip(Managers.Object.MyPlayer);
        Inventory.UpdateItemList();
        SelectedItem = null;
}

    public void UnEquipItem(Item item)
    {
        if (item == null)
        {
            Debug.Log("There is no selected item");
            return;
        }

        if (item.Object == null || item.Equiped == false)
            return;

        C_UnEquip unEquipPacket = new C_UnEquip();
        unEquipPacket.ItemName = item.ItemData.Name;
        Managers.Network.Send(unEquipPacket);

        Managers.Object.MyPlayer.RemoveEquipment(item.Object);
        Managers.Resource.Destroy(item.Object);
        item.Object = null;
        item.Equiped = false;
        Inventory.UpdateItemList();
        SelectedItem = null;
    }

    public void DropItem(Item item)
    {
        if (item == null)
        {
            Debug.Log("There is no selected item");
            return;
        }

        UnEquipItem(item);
        ItemList.Remove(item);
        Inventory.UpdateItemList();
        SelectedItem = null;
    }

    public void OpenInventory()
    {
        Inventory = Managers.UI.CreateUI("Inventory").GetComponent<Inventory>();
    }

    public void CloseInventory()
    {
        SelectedItem = null;
        Managers.UI.DestroyUI(Inventory.gameObject);
    }

    public void Clear()
    {
        ItemList.Clear();
    }
}
