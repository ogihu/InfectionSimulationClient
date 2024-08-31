using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CombineUI : MonoBehaviour
{
    TMP_Text _combineItemsText;
    ButtonUI _confirmButton;
    ButtonUI _cancelButton;

    public void Init(Inventory inventory)
    {
        _combineItemsText = Util.FindChildByName(gameObject, "CombineItemsText").GetComponent<TMP_Text>();
        _confirmButton = Util.FindChildByName(gameObject, "ConfirmButton").GetComponent<ButtonUI>();
        _cancelButton = Util.FindChildByName(gameObject, "CancelButton").GetComponent<ButtonUI>();

        _confirmButton.SetEvent(Managers.Item.Combine);
        _cancelButton.SetEvent(inventory.CancelCombine);
    }

    public void UpdateCombineItemText()
    {
        if(Managers.Item.CombineItems.Count == 0)
        {
            _combineItemsText.text = "";
            return;
        }

        _combineItemsText.text = "[ ";

        if(Managers.Item.CombineItems.Count == 1)
            _combineItemsText.text += $"{Define.ItemInfoDict[Managers.Item.CombineItems[0].ItemData.Name].Name}";
        else
        {
            _combineItemsText.text += $"{Define.ItemInfoDict[Managers.Item.CombineItems[0].ItemData.Name].Name}, {Define.ItemInfoDict[Managers.Item.CombineItems[1].ItemData.Name].Name}";
        }

        _combineItemsText.text += " ]";
    }
}
