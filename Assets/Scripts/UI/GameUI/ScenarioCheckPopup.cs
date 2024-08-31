using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScenarioCheckPopup : MonoBehaviour
{
    TMP_Text _content;

    private void Awake()
    {
        _content = Util.FindChildByName(gameObject, "Content").GetComponent<TMP_Text>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Y))
        {
            Managers.Scenario.PopupConfirm = 1;
            Managers.UI.DestroyUI(gameObject);
        }
        else if (Input.GetKeyDown(KeyCode.N))
        {
            Managers.Scenario.PopupConfirm = 2;
            Managers.UI.DestroyUI(gameObject);
        }
    }

    public void UpdateText(string content)
    {
        _content.text = content;
    }
}
