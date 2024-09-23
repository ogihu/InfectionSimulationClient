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

        //TODO : 캐릭터 상태 변경 - SweepingFloor

        return base.Use(character);
    }
}
