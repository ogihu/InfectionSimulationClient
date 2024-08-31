using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mask : Equipment
{
    public override bool Equip(BaseController character)
    {
        Transform parent = Util.FindChildByName(character.gameObject, "basic_rig Head").transform;
        if(parent == null)
        {
            Debug.LogWarning("Can't find transform");
            return false;
        }
        gameObject.transform.SetParent(parent, false);

        return base.Equip(character);
    }
}
