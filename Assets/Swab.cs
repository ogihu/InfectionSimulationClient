using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Swab : UsingItem
{
    public override bool Use(BaseController character)
    {
        StartCoroutine(Use_Swab());
        return base.Use(character);
    }

    IEnumerator Use_Swab()
    {

        yield return null;
    }
}
