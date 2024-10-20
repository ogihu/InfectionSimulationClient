using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WarningUI : MonoBehaviour
{
    TMP_Text warningText;

    private void Awake()
    {
        
    }

    public void SetText(string text)
    {
        if(warningText == null)
            warningText = Util.FindChildByName(gameObject, "WarningText").GetComponent<TMP_Text>();

        warningText.text = text;
    }
}
