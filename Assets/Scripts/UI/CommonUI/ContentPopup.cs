using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContentPopup : MonoBehaviour
{
    private void OnEnable()
    {
        Managers.UI.ContentPopups.Push(this.gameObject);
    }
}
