using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[Serializable]
public class ItemData
{
    public string Name;
    public Sprite Icon;
    public GameObject Prefab;

    public ItemData(string name)
    {
        Name = name;
        Icon = Managers.Resource.Load<Sprite>($"Sprites/ItemIcons/{Name}");
        Prefab = Managers.Resource.Load<GameObject>($"Prefabs/Equipments/{Name}");
    }
}
