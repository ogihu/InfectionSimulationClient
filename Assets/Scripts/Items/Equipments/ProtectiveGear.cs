using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProtectiveGear : Equipment
{
    public override bool Equip(BaseController character)
    {
        gameObject.transform.SetParent(character.transform, false);

        return base.Equip(character);
    }

    public override void UnEquip(BaseController character)
    {
        base.UnEquip(character);
    }
}
