using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Items : MonoBehaviour
{
    protected void Awake()
    {
        Init();
    }

    public virtual void Init() { }

    public virtual bool Equip(BaseController character)
    {
        gameObject.layer = character.gameObject.layer;
        return character.EquipItem(this.gameObject);
    }

    public virtual void UnEquip(BaseController character)
    {
        character.UnEquipItem(this.gameObject.name);
    }
}
