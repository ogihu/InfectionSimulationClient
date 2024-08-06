using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextTwinkle : MonoBehaviour
{
    TMP_Text printText;

    public void PrintText(string print, GameObject MySpeech)
    {
        StartCoroutine(Typing(print, MySpeech));
    }
    public IEnumerator Typing(string print, GameObject MySpeech)
    {
        printText = MySpeech.transform.GetChild(0).GetComponent<TMP_Text>();
        printText.text = null;
        for (int i = 0; i < print.Length; i++)
        {
            printText.text += print[i];

            yield return new WaitForSeconds(0.05f);
        }
    }
}