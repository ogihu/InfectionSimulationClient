using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProtectiveGear : Equipment
{
    public override void Equip(BaseController character)
    {
        base.Equip(character);

        Transform parent = Util.FindChildByName(character.gameObject, "basic_rig Pelvis").transform;
        if (parent == null)
        {
            Debug.Log("Can't find transform");
            return;
        }

        gameObject.transform.SetParent(parent.transform, false);
    }

    public override void UnEquip(BaseController character)
    {
        base.UnEquip(character);
    }
}
