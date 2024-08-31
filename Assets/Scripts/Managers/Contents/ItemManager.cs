using Google.Protobuf.Protocol;
using System;
using System.Collections;
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

public class ItemManager
{
    public List<Item> ItemList = new List<Item>();
    public Inventory Inventory { get; set; }
    public Item SelectedItem { get; set; }

    public bool IsCombining { get; set; }
    public List<Item> CombineItems = new List<Item>();

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

        if (item.ItemData.Prefab == null)
        {
            Managers.UI.CreateSystemPopup("WarningPopup", "착용할 수 있는 장비가 아닙니다.");
            return;
        }

        //이미 착용한 장비일 경우 취소
        if (item.Equiped == true || item.Object != null)
        {
            Managers.UI.CreateSystemPopup("WarningPopup", "이미 착용하고 있는 장비입니다.");
            return;
        }

        if(Managers.Scenario.CurrentScenarioInfo == null)
        {
            Managers.UI.CreateSystemPopup("WarningPopup", "장비를 착용할 수 있는 상황이 아닙니다.");
            return;
        }

        if (Managers.Scenario.CurrentScenarioInfo.Position != Managers.Object.MyPlayer.Position || Managers.Scenario.CurrentScenarioInfo.Action != "Equip")
        {
            Managers.UI.CreateSystemPopup("WarningPopup", "장비를 착용할 수 있는 상황이 아닙니다.");
            return;
        }

        if (Managers.Scenario.CurrentScenarioInfo.Equipment != item.ItemData.Name)
        {
            Managers.UI.CreateSystemPopup("WarningPopup", "현재 필요한 장비가 아닙니다.");
            return;
        }

        item.Object = Managers.Resource.Instantiate(item.ItemData.Prefab);
        Equipment equipment = item.Object.GetComponent<Equipment>();

        if (equipment == null)
        {
            Debug.Log("Can't find the component : Equipment");
            return;
        }

        if (!equipment.Equip(Managers.Object.MyPlayer))
            Managers.Resource.Destroy(item.Object);

        item.Equiped = true;

        C_Equip equipPacket = new C_Equip();
        equipPacket.ItemName = item.ItemData.Name;
        Managers.Network.Send(equipPacket);

        Inventory.UpdateItemList();
        Managers.Scenario.MyAction = "Equip";
        Managers.Scenario.Equipment = item.ItemData.Name;
        SelectedItem = null;
        Inventory.ChangeItemText("");
}

    public void StartCombineItem()
    {
        SelectedItem = null;
        IsCombining = true;
        CombineItems.Clear();
    }

    public void Combine()
    {
        if(CombineItems.Count < 2)
        {
            Managers.UI.CreateSystemPopup("WarningPopup", "아이템이 선택되지 않았습니다.");
            return;
        }

        Managers.UI.CreateSystemPopup("WarningPopup", "결합할 수 없는 아이템 입니다.");
        //결합할 수 있는 아이템은 결합하여 인벤토리에 새로 저장, 기존 아이템은 삭제?
        //결합할 수 없는 아이템은 결합할 수 없다는 UI 띄우기
    }

    public void CancelCombining()
    {
        IsCombining = false;
        CombineItems.Clear();
    }

    public void UnEquipItem(Item item)
    {
        if (item == null)
        {
            Debug.Log("There is no selected item");
            return;
        }

        //선택한 장비를 착용하고 있지 않을 경우 취소
        if (item.Object == null || item.Equiped == false)
        {
            Managers.UI.CreateSystemPopup("WarningPopup", "해당 장비를 착용하고 있지 않습니다.");
            return;
        }

        //시나리오 상 본인의 차례가 아니거나, 장비를 해제할 단계가 아닐 경우 취소
        if (Managers.Scenario.CurrentScenarioInfo.Position != Managers.Object.MyPlayer.Position || Managers.Scenario.CurrentScenarioInfo.Action != "UnEquip")
        {
            Managers.UI.CreateSystemPopup("WarningPopup", "장비를 해제할 수 있는 상황이 아닙니다.");
            return;
        }

        //시나리오 상 정해진 위치에서 탈의를 해야 할 경우
        if (!Managers.Scenario.CheckPlace())
        {
            Managers.UI.CreateSystemPopup("WarningPopup", "올바른 장소에서 장비를 해제하세요.");
            return;
        }

        C_UnEquip unEquipPacket = new C_UnEquip();
        unEquipPacket.ItemName = item.ItemData.Name;
        Managers.Network.Send(unEquipPacket);

        Equipment equipment = item.Object.GetComponent<Equipment>();

        equipment.UnEquip(Managers.Object.MyPlayer);
        item.Object = null;
        item.Equiped = false;
        Inventory.UpdateItemList();
        SelectedItem = null;
        Inventory.ChangeItemText("");
        Managers.Scenario.MyAction = "UnEquip";
        Managers.Scenario.Equipment = null;
    }

    public void DropItem(Item item)
    {
        if (item == null)
        {
            Debug.Log("There is no selected item");
            return;
        }

        if(Managers.Scenario.CurrentScenarioInfo == null)
        {
            Managers.UI.CreateSystemPopup("WarningPopup", "장비를 버릴 수 있는 상황이 아닙니다.");
            return;
        }

        //시나리오 상 본인의 차례가 아니거나, 장비를 해제할 단계가 아닐 경우 취소
        if (Managers.Scenario.CurrentScenarioInfo.Position != Managers.Object.MyPlayer.Position || Managers.Scenario.CurrentScenarioInfo.Action != "UnEquip")
        {
            Managers.UI.CreateSystemPopup("WarningPopup", "장비를 버릴 수 있는 상황이 아닙니다.");
            return;
        }

        if (!Managers.Scenario.CheckPlace())
        {
            Debug.Log("올바른 장소에서 버리세요.");
            return;
        }

        UnEquipItem(item);
        ItemList.Remove(item);
        Inventory.UpdateItemList();
        SelectedItem = null;
        Inventory.ChangeItemText("");
    }

    public void OpenInventory()
    {
        Managers.Object.MyPlayer.State = CreatureState.UsingInventory;

        if (Inventory == null)
        {
            Inventory = Managers.UI.CreateUI("Inventory").GetComponent<Inventory>();
        }
        else
        {
            Debug.Log("Inventory is already open.");
        }
    }

    public void CloseInventory()
    {
        Managers.Object.MyPlayer.State = CreatureState.Idle;

        if (Inventory != null)
        {
            SelectedItem = null;
            Managers.UI.DestroyUI(Inventory.gameObject);
            Inventory = null;
        }
    }

    public void Clear()
    {
        ItemList.Clear();
    }
}
