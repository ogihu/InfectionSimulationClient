using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EMRCheck : MonoBehaviour
{
    bool _isRight = false;

    protected virtual void Awake()
    {
        GetComponent<Toggle>().onValueChanged.AddListener(OnValueChanged);
    }

    protected virtual void OnValueChanged(bool isOn)
    {
        if (_isRight)
        {
            GetComponent<Toggle>().isOn = true;
            return;
        }

        if (isOn)
        {
            string objectName = gameObject.name;

            switch(objectName)
            {
                case "RightCheck":
                    GameObject.Find("MpoxEMRWrite").GetComponent<EMRWrite>().IncreaseCount();
                    _isRight = true;
                    break;
                case "FaultCheck":
                    Managers.UI.CreateSystemPopup("WarningPopup", "올바르지 않은 선택입니다.", UIManager.NoticeType.Warning);
                    GetComponent<Toggle>().isOn = false;
                    break;
            }
        }
    }
}
