using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DropdownMic : MonoBehaviour
{
    public TMP_Dropdown micDropdown; 

    void Awake()
    {
        micDropdown = GetComponent<TMP_Dropdown>();
        // Dropdown을 초기화
        micDropdown.ClearOptions();

        // 마이크 장치 목록 가져오기
        string[] devices = Microphone.devices;

        if (devices.Length > 0)
        {
            // Dropdown에 마이크 장치 추가
            micDropdown.AddOptions(new List<string>(devices));

            // 기본적으로 첫 번째 마이크 장치를 선택
            if(Managers.Setting.SelectedMic == null)
                Managers.Setting.SelectedMic = devices[0];

            // Dropdown에 값 변경 리스너 추가
            micDropdown.onValueChanged.AddListener(delegate { MicDropdownValueChanged(); });
        }
        else
        {
            Debug.LogWarning("마이크가 연결되어 있지 않습니다.");
        }
    }

    void MicDropdownValueChanged()
    {
        // 사용자가 선택한 마이크 장치를 업데이트
        Managers.Setting.SelectedMic = micDropdown.options[micDropdown.value].text;
        Debug.Log("선택된 마이크: " + Managers.Setting.SelectedMic);
    }
}
