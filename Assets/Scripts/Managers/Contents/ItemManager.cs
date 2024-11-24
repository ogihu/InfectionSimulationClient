using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ItemInfo
{
    public ItemData ItemData { get; set; }
    public GameObject Object { get; set; }
    public bool Using { get; set; }
}

public class ItemManager
{
    public List<ItemInfo> ItemList = new List<ItemInfo>();
    public Inventory Inventory { get; set; }
    public ItemInfo SelectedItem { get; set; }

    public bool IsInventoryOpen {
        get
        {
            if (Inventory == null)
                return false;

            return Inventory.gameObject.activeSelf;
        }
    }

    public bool IsCombining { get; set; }
    public List<ItemInfo> CombineItems = new List<ItemInfo>();

    public void GetItem(ItemData itemData)
    {
        ItemInfo item = new ItemInfo();
        item.ItemData = itemData;
        item.Using = false;

        ItemList.Add(item);

        Managers.UI.CreateSystemPopup("WarningPopup", $"{Define.ItemInfoDict[itemData.Name].Name} 아이템을 획득하였습니다.", UIManager.NoticeType.Info);
        Debug.Log($"아이템 획득 - 현재 보유한 아이템 {ItemList.Count}개");
    }

    public void EquipItem(ItemInfo item)
    {
        if(item == null)
        {
            Debug.Log("There is no selected item");
            return;
        }

        if (item.ItemData.Prefab == null)
        {
            Managers.UI.CreateSystemPopup("WarningPopup", "착용할 수 있는 장비가 아닙니다.", UIManager.NoticeType.Warning);
            return;
        }

        //이미 착용한 장비일 경우 취소
        if (item.Using == true || item.Object != null)
        {
            Managers.UI.CreateSystemPopup("WarningPopup", "이미 착용하고 있는 장비입니다.", UIManager.NoticeType.Warning);
            return;
        }

        if(Managers.Scenario.CurrentScenarioInfo == null)
        {
            Managers.UI.CreateSystemPopup("WarningPopup", "장비를 착용할 수 있는 상황이 아닙니다.", UIManager.NoticeType.Warning);
            return;
        }

        if(Managers.Scenario.CurrentScenarioInfo.Position != Managers.Object.MyPlayer.Position)
        {
            Managers.UI.CreateSystemPopup("WarningPopup", "장비를 사용할 수 있는 상황이 아닙니다.", UIManager.NoticeType.Warning);
            return;
        }

        if (Managers.Scenario.CurrentScenarioInfo.Action != "Use" && Managers.Scenario.CurrentScenarioInfo.Action != "BloodCollection")
        {
            Managers.UI.CreateSystemPopup("WarningPopup", "장비를 사용할 수 있는 상황이 아닙니다.", UIManager.NoticeType.Warning);
            return;
        }

        if (Managers.Scenario.CurrentScenarioInfo.Item != item.ItemData.Name)
        {
            Managers.UI.CreateSystemPopup("WarningPopup", "현재 필요한 아이템이 아닙니다.", UIManager.NoticeType.Warning);
            return;
        }

        if (!Managers.Scenario.CheckPlace())
        {
            return;
        }

        item.Object = Managers.Resource.Instantiate(item.ItemData.Prefab);
        Item selectedItem = item.Object.GetComponent<Item>();

        if (selectedItem == null)
        {
            Debug.Log("Can't find the component : Item");
            return;
        }

        if (!selectedItem.Use(Managers.Object.MyPlayer))
        {
            Managers.Resource.Destroy(item.Object);
            return;
        }

        if(!(selectedItem as ImmediatelyUsingItem))
            item.Using = true;
 
        selectedItem.ItemInfo = item;
        C_Equip equipPacket = new C_Equip();
        equipPacket.ItemName = item.ItemData.Name;
        Managers.Network.Send(equipPacket);

        Inventory.UpdateItemList();
        Managers.Scenario.MyAction = "Use";
        Managers.Scenario.Item = item.ItemData.Name;
        SelectedItem = null;
        Inventory.ChangeItemText("");
        Managers.UI.CreateSystemPopup("WarningPopup", $"{Define.ItemInfoDict[item.ItemData.Name].Name}을 사용하였습니다.", UIManager.NoticeType.Info);

        if(selectedItem as ImmediatelyUsingItem)
        {
            Managers.Resource.Destroy(item.Object);
            item.Object = null;
        }

        if(selectedItem as Syringe || selectedItem as DrySwab)
        {
            Managers.Item.CloseInventory();
        }
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
            Managers.UI.CreateSystemPopup("WarningPopup", "아이템이 선택되지 않았습니다.", UIManager.NoticeType.None);
            return;
        }

        Managers.UI.CreateSystemPopup("WarningPopup", "결합할 수 없는 아이템 입니다.", UIManager.NoticeType.None);
        //결합할 수 있는 아이템은 결합하여 인벤토리에 새로 저장, 기존 아이템은 삭제?
        //결합할 수 없는 아이템은 결합할 수 없다는 UI 띄우기
    }

    public void CancelCombining()
    {
        IsCombining = false;
        CombineItems.Clear();
    }

    public void UnEquipItem(ItemInfo item)
    {
        if (item == null)
        {
            Debug.Log("There is no selected item");
            return;
        }

        //선택한 장비를 착용하고 있지 않을 경우 취소
        if (item.Object == null || item.Using == false)
        {
            Managers.UI.CreateSystemPopup("WarningPopup", "해당 장비를 사용하고 있지 않습니다.", UIManager.NoticeType.None);
            return;
        }

        //시나리오 상 본인의 차례가 아니거나, 장비를 해제할 단계가 아닐 경우 취소
        if (Managers.Scenario.CurrentScenarioInfo.Position != Managers.Object.MyPlayer.Position)
        {
            Managers.UI.CreateSystemPopup("WarningPopup", "해제할 수 있는 상황이 아닙니다.", UIManager.NoticeType.None);
            return;
        }

        if (Managers.Scenario.CurrentScenarioInfo.Action != "UnUse")
        {
            Managers.UI.CreateSystemPopup("WarningPopup", "해제할 수 있는 상황이 아닙니다.", UIManager.NoticeType.None);
            return;
        }

        if(item.ItemData.Name != Managers.Scenario.CurrentScenarioInfo.Item)
        {
            Managers.UI.CreateSystemPopup("WarningPopup", "현재 해제가 필요한 장비가 아닙니다.", UIManager.NoticeType.Warning);
            return;
        }

        //시나리오 상 정해진 위치에서 탈의를 해야 할 경우
        if (!Managers.Scenario.CheckPlace())
        {
            return;
        }

        ForceUnUseItem(item);

        Managers.Scenario.MyAction = "UnUse";
        Managers.UI.CreateSystemPopup("WarningPopup", $"{Define.ItemInfoDict[item.ItemData.Name].Name}을 해제하였습니다", UIManager.NoticeType.Info);
    }

    /// <summary>
    /// 아이템 강제 사용 해제
    /// </summary>
    /// <param name="item"></param>
    public void ForceUnUseItem(ItemInfo item)
    {
        Item selectedItem = item.Object.GetComponent<Item>();

        selectedItem.UnUse();
        item.Object = null;
        item.Using = false;
        if(Inventory != null)
        {
            Inventory.UpdateItemList();
            Inventory.ChangeItemText("");
        }

        SelectedItem = null;
        Managers.Scenario.Item = item.ItemData.Name;
    }

    public void ForceDropItem(ItemInfo item)
    {
        ForceUnUseItem(item);
        ItemList.Remove(item);
        SelectedItem = null;
    }

    public void DropItem(ItemInfo item)
    {
        if (item == null)
        {
            Debug.Log("There is no selected item");
            return;
        }

        if(Managers.Scenario.CurrentScenarioInfo == null)
        {
            Managers.UI.CreateSystemPopup("WarningPopup", "시나리오가 시작되지 않았습니다.", UIManager.NoticeType.None);
            return;
        }

        //시나리오 상 본인의 차례가 아니거나, 장비를 해제할 단계가 아닐 경우 취소
        if (Managers.Scenario.CurrentScenarioInfo.Position != Managers.Object.MyPlayer.Position || Managers.Scenario.CurrentScenarioInfo.Action != "UnEquip")
        {
            Managers.UI.CreateSystemPopup("WarningPopup", "장비를 버릴 수 있는 상황이 아닙니다.", UIManager.NoticeType.None);
            return;
        }

        if (!Managers.Scenario.CheckPlace())
        {
            return;
        }

        UnEquipItem(item);
        ItemList.Remove(item);
        Inventory.UpdateItemList();
        SelectedItem = null;
        Inventory.ChangeItemText("");
    }

    public void OpenOrCloseInventory()
    {
        //인벤토리가 열려있으면 닫기
        if (IsInventoryOpen)
        {
            CloseInventory();
        }
        //인벤토리가 닫혀있으면 열기
        else
        {
            OpenInventory();
        }
    }

    public void OpenInventory()
    {
        if (Inventory == null)
        {
            Inventory = Managers.UI.CreateUI("Inventory").GetComponent<Inventory>();
        }

        Inventory.UpdateItemList();
        Inventory.gameObject.SetActive(true);
    }

    public void CloseInventory()
    {
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
        Inventory = null;
        SelectedItem = null;
        CombineItems.Clear();
    }
}
