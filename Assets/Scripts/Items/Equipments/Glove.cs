using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Glove : TwinEquipment
{
    public override bool Use(BaseController character)
    {
        Transform leftParent = Util.FindChildByName(character.gameObject, "basic_rig L Hand").transform;
        Transform rightParent = Util.FindChildByName(character.gameObject, "basic_rig R Hand").transform;

        if (leftParent == null || rightParent == null)
        {
            Debug.LogWarning("Can't find transform");
            return false;
        }

        _leftEquipment.transform.SetParent(leftParent, false);
        _rightEquipment.transform.SetParent(rightParent, false);

        return base.Use(character);
    }
}
