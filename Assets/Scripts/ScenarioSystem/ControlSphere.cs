using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlSphere : MonoBehaviour
{
    public string _message;
    GameObject _popup;

    private void Awake()
    {
        _message = "<color=#FF0000>격리 환자를 이송 중 입니다.\n가까이 접근하지 마세요.</color>";
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("MyPlayer"))
            return;

        _popup = Managers.UI.CreateSystemPopup("PopupNotice", _message, UIManager.NoticeType.Warning, UIManager.PopupType.ManualDestroy);
    }

    private void OnTriggerStay(Collider other)
    {
        
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("MyPlayer"))
            return;

        if (_popup != null)
            _popup.SetActive(false);
    }

    private void OnDisable()
    {
        if (_popup == null)
            return;

        if(_popup.activeSelf)
            _popup.SetActive(false);
    }
}
