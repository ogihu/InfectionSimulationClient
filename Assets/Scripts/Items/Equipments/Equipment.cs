using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Equipment : Items
{
    public override bool Equip(BaseController character)
    {
        gameObject.layer = character.gameObject.layer;
        return character.EquipItem(this.gameObject);
    }

    public override void UnEquip(BaseController character)
    {
        character.UnEquipItem(this.gameObject.name);
    }
}