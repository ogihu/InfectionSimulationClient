using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PanelChatUI : MonoBehaviour
{
    Transform _target;


    public void Init(Transform target, string chat)
    {
        _target = target;
        GetComponentInChildren<TMP_Text>().text = chat;
    }

    private void Update()
    {
        ChasingTarget();
    }

    void ChasingTarget()
    {
        if (_target == null)
            Debug.LogError("There is no target to chase, Check the target of this object");

        if (GetComponent<RectTransform>().anchoredPosition3D == _target.position)
            return;

        GetComponent<RectTransform>().anchoredPosition3D = new Vector3(_target.position.x, _target.position.y + 2.5f, _target.position.z);
        Vector3 dir = GetComponent<RectTransform>().anchoredPosition3D - Camera.main.transform.position;
        transform.rotation = Quaternion.LookRotation(dir);
    }
}
