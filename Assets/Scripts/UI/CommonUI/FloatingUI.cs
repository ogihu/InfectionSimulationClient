using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class FloatingUI : MonoBehaviour
{
    Canvas _canvas;
    Transform _target;
    float _height = 1.0f;
    protected bool _isStatic = false;

    public virtual void Init(Transform target, float height = 1.0f, bool isStatic = false) // 높이 기본값을 수정합니다.
    {
        _target = target;
        _height = height;
        _isStatic = isStatic;

        if (_canvas == null)
        {
            _canvas = GetComponent<Canvas>();
        }
    }

    public virtual void ChangeMessage(string chat)
    {
        TMP_Text textComponent = GetComponentInChildren<TMP_Text>();
        if (textComponent != null)
        {
            textComponent.text = chat;
        }
        else
        {
            Debug.LogWarning("TMP_Text 컴포넌트를 찾을 수 없습니다.");
        }
    }

    private void Update()
    {
        if (_isStatic)
            return;

        ChasingTarget();
    }

    void ChasingTarget()
    {
        if (_target == null)
        {
            Debug.LogError("There is no target to chase, Check the target of this object");
            return;
        }

        if (_canvas == null)
        {
            Debug.LogError("There is no Canvas attached to this object, Check the Canvas component");
            return;
        }

        Vector3 targetPosition = _target.position + new Vector3(0, _height, 0);

        if (GetComponent<RectTransform>().position != targetPosition)
        {
            GetComponent<RectTransform>().position = targetPosition;

            Vector3 dir = GetComponent<RectTransform>().position - Camera.main.transform.position;
            dir = new Vector3(dir.x, 0, dir.z);
            Quaternion rotation = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, rotation.eulerAngles.y, transform.rotation.eulerAngles.z);

            if (_canvas != null)
            {
                int distance = (int)(Camera.main.transform.position - transform.position).magnitude;
                if (distance == 0)
                    _canvas.sortingOrder = 0;
                else
                    _canvas.sortingOrder = (1 / distance) * 100;
            }
            else
            {
                Debug.LogWarning("Canvas 컴포넌트를 찾을 수 없습니다. sortingOrder를 설정할 수 없습니다.");
            }
        }
    }
}
