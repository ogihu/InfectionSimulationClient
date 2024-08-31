using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Whisper.Utils;

public class DropdownSelectMic : MonoBehaviour
{
    void Start()
    {
        GameObject.Find("RealtimeSTT").GetComponent<LoopingMicrophone>().GetMics(gameObject.GetComponent<TMP_Dropdown>());
    }
}
