using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class FloatingUI : MonoBehaviour
{
    protected Canvas _canvas;
    protected Transform _target;
    protected bool _isStatic = false;
    protected float _height = 1.0f;
    protected float _width = 0.0f;
    protected float _length = 0.0f;

    public virtual void Init(Transform target, float x = 0.0f, float y = 1.0f, float z = 0.0f ,bool isStatic = false)
    {
        _target = target;
        _isStatic = isStatic;
        _width = x;
        _height = y;
        _length = z;

        transform.position = new Vector3(target.position.x + _width, target.position.y + _height, target.position.z + _length);

        if (_canvas == null)
        {
            _canvas = GetComponent<Canvas>();
        }
    }

    public void SetDescription(string content)
    {
        if (content == null)
            return;

        GetComponentInChildren<TMP_Text>().text = content;
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
        UpdateRotation();

        if (_isStatic)
            return;

        ChasingTarget();
    }

    public virtual void ChasingTarget()
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

        Vector3 targetPosition = _target.position + new Vector3(_width, _height, _length);

        if (GetComponent<RectTransform>().position != targetPosition)
        {
            GetComponent<RectTransform>().position = targetPosition;
        }

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

    void UpdateRotation()
    {
        Vector3 dir = GetComponent<RectTransform>().position - Camera.main.transform.position;
        dir = new Vector3(dir.x, 0, dir.z);
        Quaternion rotation = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
    }

    public void SetHeight(float height)
    {
        _height = height;
    }

}
