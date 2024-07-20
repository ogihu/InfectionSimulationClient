using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public class InteractableObject : MonoBehaviour
{
    [SerializeField] ItemData _itemData;

    private void Awake()
    {
        _itemData = new ItemData(gameObject.name);
    }

    public void GetItem()
    {
        Managers.Inventory.GetItem(_itemData);
    }
}
