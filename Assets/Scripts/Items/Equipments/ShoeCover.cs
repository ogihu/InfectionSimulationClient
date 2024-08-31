using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShoeCover : TwinEquipment
{
    public override bool Equip(BaseController character)
    {
        Transform leftFoot = Util.FindChildByName(character.gameObject, "basic_rig L Foot").transform;
        Transform rightFoot = Util.FindChildByName(character.gameObject, "basic_rig R Foot").transform;

        if (leftFoot == null || rightFoot == null)
        {
            Debug.LogWarning("Can't find transform");
            return false;
        }

        _leftEquipment.transform.SetParent(leftFoot, false);
        _rightEquipment.transform.SetParent(rightFoot, false);

        return base.Equip(character);
    }
}
