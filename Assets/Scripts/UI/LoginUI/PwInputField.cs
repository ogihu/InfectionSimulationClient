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
        _inputField.onValueChanged.AddListener(OnInputValueChanged);
    }

    void OnInputValueChanged(string input)
    {
        // 입력된 문자열을 한 글자씩 영어로 변환
        char[] convertedChars = new char[input.Length];
        for (int i = 0; i < input.Length; i++)
        {
            convertedChars[i] = Define.ChangeKtoE(input[i]);
        }

        // 변환된 문자열을 InputField에 반영
        string convertedText = new string(convertedChars);
        if (input != convertedText)
        {
            _inputField.text = convertedText;
            _inputField.caretPosition = convertedText.Length;
        }
    }
}
