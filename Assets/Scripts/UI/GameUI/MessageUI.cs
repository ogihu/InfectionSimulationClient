using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MessageUI : MonoBehaviour
{
    private void OnDisable()
    {
        if (ButtonMessage.Message != null)
        {
            ButtonMessage.Message = null;
        }
    }
}
