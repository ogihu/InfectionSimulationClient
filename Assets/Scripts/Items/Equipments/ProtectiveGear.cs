using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProtectiveGear : Equipment
{
    public override bool Use(BaseController character)
    {
        gameObject.transform.SetParent(character.transform, false);

        return base.Use(character);
    }

    public override void UnUse()
    {
        base.UnUse();
    }
}
