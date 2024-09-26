using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WetMop : UsingItem
{
    public override bool Use(BaseController character)
    {
        Transform parent = Util.FindChildByName(character.gameObject, "R_hand_grap_point").transform;
        if (parent == null)
        {
            Debug.LogWarning("Can't find transform");
            return false;
        }
        gameObject.transform.SetParent(parent, false);

        character.State = Google.Protobuf.Protocol.CreatureState.SweepingFloor;

        return base.Use(character);
    }
}
