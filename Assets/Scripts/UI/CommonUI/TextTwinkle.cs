using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextTwinkle : MonoBehaviour
{
    TMP_Text printText;
    public bool IsPrinting { get; private set; }

    public IEnumerator Typing(string print, GameObject text)
    {
        IsPrinting = true;
        bool colorOption = false;

        printText = text.GetComponent<TMP_Text>();
        printText.text = null;

        for (int i = 0; i < print.Length; i++)
        {
            if (print[i] == '<')
            {
                colorOption = true;
            }

            if (!colorOption)
            {
                printText.text += print[i];
                yield return new WaitForSeconds(0.03f);
            }
            else
            {
                printText.text += print[i];
            }

            if (print[i] == '>')
            {
                colorOption = false;
            }
        }

        IsPrinting = false;
        Debug.Log($"텍스트 : \n{printText.text}");
    }

    public IEnumerator SelectedWordsLoop(string staticWords, string dynamicWords, GameObject text)
    {
        IsPrinting = true;

        printText = text.GetComponent<TMP_Text>();

        while (true)
        {
            printText.text = staticWords;

            for(int i = 0; i < dynamicWords.Length; i++)
            {
                yield return new WaitForSeconds(0.5f);
                printText.text += dynamicWords[i];
            }

            yield return new WaitForSeconds(0.5f);
        }
    }
}