using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FloatingUI : MonoBehaviour
{
    Canvas _canvas;
    Transform _target;
    float _height = 3.5f;

    public void Init(Transform target, float height = 3.5f)
    {
        _target = target;
        _height = height;
        _canvas = GetComponent<Canvas>();
    }

    public virtual void ChangeMessage(string chat)
    {
        GetComponentInChildren<TMP_Text>().text = chat;
    }

    private void Update()
    {
        ChasingTarget();
    }

    void ChasingTarget()
    {
        if (_target == null)
        {
            Debug.LogError("There is no target to chase, Check the target of this object");
            return;
        }

        if (GetComponent<RectTransform>().anchoredPosition3D == _target.position)
            return;

        GetComponent<RectTransform>().position = new Vector3(_target.position.x, _target.position.y + _height, _target.position.z);
        Vector3 dir = GetComponent<RectTransform>().position - Camera.main.transform.position;
        dir = new Vector3(dir.x, 0, dir.z);
        Quaternion rotation = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, rotation.eulerAngles.y, transform.rotation.eulerAngles.z);

        int distance = (int)(Camera.main.transform.position - transform.position).magnitude;
        if (distance == 0)
            _canvas.sortingOrder = 0;
        else
            _canvas.sortingOrder = (1 / distance) * 100;
    }
}
