using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Whisper.Utils.Extension;

public class DropdownSelectMic : MonoBehaviour
{
    void Start()
    {
        GameObject.Find("SpeechRecognitor").GetComponent<MicRecord>().GetMics(gameObject.GetComponent<TMP_Dropdown>());
    }
}
