using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Handwash : UsingItem
{
    public override bool Use(BaseController character)
    {
        if (Managers.Scenario.CurrentScenarioInfo != null)
            Managers.Scenario.Item = "Handwash";

        return base.Use(character);
    }
}
