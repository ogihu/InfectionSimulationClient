using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Popup : MonoBehaviour
{
    private void OnEnable()
    {
        Managers.UI.PopupStack.Push(this.gameObject);
    }
}
