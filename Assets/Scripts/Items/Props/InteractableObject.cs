using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public class InteractableObject : MonoBehaviour
{
    [SerializeField] ItemData _itemData;
    FloatingUI _interactionKey;

    private void Awake()
    {
        _itemData = new ItemData(gameObject.name);
        _interactionKey = Managers.UI.CreateUI("InteractionKey", UIManager.CanvasType.World).GetComponent<FloatingUI>();
        _interactionKey.Init(transform, 0.2f);
        InActiveKeyUI();
    }

    public void ActiveKeyUI()
    {
        _interactionKey.gameObject.SetActive(true);
    }

    public void InActiveKeyUI()
    {
        _interactionKey.gameObject.SetActive(false);
    }

    public void GetItem()
    {
        Managers.Inventory.GetItem(_itemData);
    }
}
