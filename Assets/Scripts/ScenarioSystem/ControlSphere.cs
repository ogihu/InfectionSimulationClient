using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlSphere : MonoBehaviour
{
    public string message;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("MyPlayer"))
            return;

        Managers.UI.CreateScenarioPopup(message, UIManager.PopupType.ManualDestroy);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("MyPlayer"))
            return;

        if (Managers.UI.ScenarioPopup != null)
            Managers.UI.DestroyUI(Managers.UI.ScenarioPopup);
    }
}
