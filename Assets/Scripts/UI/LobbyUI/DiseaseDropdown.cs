using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DiseaseDropdown : MonoBehaviour
{
    public TMP_Dropdown diseasesDropdown;

    void Awake()
    {
        diseasesDropdown = GetComponent<TMP_Dropdown>();
        // Dropdown을 초기화
        diseasesDropdown.ClearOptions();

        // 감염병 목록 가져오기
        string[] diseases = Define.Diseases;

        if(diseases.Length <= 0)
        {
            Debug.LogError("감염병이 정의되어 있지 않습니다.");
            return;
        }

        if (diseases.Length > 0)
        {
            diseasesDropdown.AddOptions(new List<string>(diseases));

            // Dropdown에 값 변경 리스너 추가
            diseasesDropdown.onValueChanged.AddListener(delegate { DropdownValueChanged(); });
        }
        else
        {
            Debug.LogWarning("마이크가 연결되어 있지 않습니다.");
        }
    }

    void DropdownValueChanged()
    {

    }
}
