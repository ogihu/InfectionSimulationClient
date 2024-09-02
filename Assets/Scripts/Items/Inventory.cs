using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
    Transform _content;
    
    TMP_Text _itemName;
    TMP_Text _itemInfo;
    ButtonInventory _equipButton;
    ButtonInventory _unEquipButton;
    ButtonUI _combineButton;
    ButtonInventory _dropButton;
    ButtonUI _exitButton;
    CombineUI _combineUI;
    RawImage _myCharacter;
    Camera _tpCam;

    List<ButtonItemSlot> _itemSlots = new List<ButtonItemSlot>();

    private void Awake()
    {
        _content = Util.FindChildByName(gameObject, "Content").transform;
        _itemName = Util.FindChildByName(gameObject, "ItemName").GetComponent<TMP_Text>();
        _itemInfo = Util.FindChildByName(gameObject, "ItemInfo").GetComponent<TMP_Text>();
        _equipButton = Util.FindChildByName(gameObject, "EquipButton").GetComponent<ButtonInventory>();
        _unEquipButton = Util.FindChildByName(gameObject, "UnEquipButton").GetComponent<ButtonInventory>();
        _combineButton = Util.FindChildByName(gameObject, "CombineButton").GetComponent<ButtonUI>();
        _dropButton = Util.FindChildByName(gameObject, "DropButton").GetComponent<ButtonInventory>();
        _combineUI = Util.FindChildByName(gameObject, "CombineUI").GetComponent<CombineUI>();
        _myCharacter = Util.FindChildByName(gameObject, "ThirdPersonView").GetComponent<RawImage>();
        _exitButton = Util.FindChildByName(gameObject, "ExitUI").GetComponent<ButtonUI>();

        _equipButton.SetEvent(Managers.Item.EquipItem);
        _unEquipButton.SetEvent(Managers.Item.UnEquipItem);
        _combineButton.SetEvent(StartCombine);
        _dropButton.SetEvent(Managers.Item.DropItem);
        _exitButton.SetEvent(Managers.Item.CloseInventory);

        _combineUI.Init(this);
    }

    public void StartCombine()
    {
        Managers.Item.StartCombineItem();
        UpdateCombineItemsText();
        _combineUI.gameObject.SetActive(true);
    }

    public void CancelCombine()
    {
        Managers.Item.CancelCombining();
        _combineUI.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        _tpCam = Managers.Resource.Instantiate("System/ThirdPersonCamera").GetComponent<Camera>();
        Vector3 camPos = Managers.Object.MyPlayer.transform.position + Managers.Object.MyPlayer.transform.forward * 2;
        camPos.y += 1;
        _tpCam.transform.position = camPos;
        Vector3 lookAt = Managers.Object.MyPlayer.transform.position + Managers.Object.MyPlayer.transform.up;
        _tpCam.transform.LookAt(lookAt);
        _tpCam.targetTexture = Managers.Resource.Load<RenderTexture>("Textures/RenderTextures/ThirdPersonRenderTexture");
        _myCharacter.texture = _tpCam.targetTexture;
        _itemName.text = "";
        _itemInfo.text = "";
        UpdateItemList();
    }

    private void OnDisable()
    {
        CancelCombine();
        Managers.Resource.Destroy(_tpCam.gameObject);
    }

    public void UpdateCombineItemsText()
    {
        _combineUI.UpdateCombineItemText();
    }

    public void UpdateItemList()
    {
        int i = 0;
        for (; i < Managers.Item.ItemList.Count; i++)
        {
            if (i < _itemSlots.Count)
            {
                _itemSlots[i].SetItem(Managers.Item.ItemList[i]);
                Button button = _itemSlots[i].GetComponent<Button>();
                button.transition = Selectable.Transition.None;
                ColorBlock colors = button.colors;
                colors.normalColor = Color.white; // 기본 상태의 색상 설정
                colors.highlightedColor = Color.white; // 강조된 상태의 색상 설정
                colors.pressedColor = Color.gray; // 눌린 상태의 색상 설정
                colors.selectedColor = Color.gray; // 선택된 상태의 색상 설정
                colors.disabledColor = Color.gray; // 비활성화된 상태의 색상 설정
                button.colors = colors;
                button.transition = Selectable.Transition.ColorTint;
            }
            else
            {
                ButtonItemSlot itemSlot = Managers.UI.CreateUI("ItemSlot", _content).GetComponent<ButtonItemSlot>();
                itemSlot.SetItem(Managers.Item.ItemList[i]);
                _itemSlots.Add(itemSlot);
            }
        }

        for (; i < _itemSlots.Count; i++)
        {
            Managers.UI.DestroyUI(_itemSlots[i].gameObject);
            _itemSlots.Remove(_itemSlots[i]);
        }

        Debug.Log($"아이템 슬롯 업데이트 - 보유한 아이템 {Managers.Item.ItemList.Count}개, 슬롯 {_itemSlots.Count}개");
    }

    public void ChangeItemText(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            _itemName.text = "";
            _itemInfo.text = "";
            return;
        }

        _itemName.text = Define.ItemInfoDict[name].Name;
        _itemInfo.text = Define.ItemInfoDict[name].Description;
    }
}
