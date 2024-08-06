using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScenarioTest : MonoBehaviour
{
    private void Update()
    {
        GetStartButton();
    }

    void GetStartButton()
    {
        if (Input.GetKeyDown(KeyCode.Y))
        {
            Managers.Scenario.SendScenarioInfo("¿¥Æø½º");
        }
    }
}
