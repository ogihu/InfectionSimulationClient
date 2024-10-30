using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PwInputField : MonoBehaviour
{
    TMP_InputField _inputField;

    private void Awake()
    {
        _inputField = GetComponent<TMP_InputField>();
    }
}
