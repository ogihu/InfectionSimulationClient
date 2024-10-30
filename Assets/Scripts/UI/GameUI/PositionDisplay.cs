using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionDisplay : FloatingUI
{
    public override void ChasingTarget()
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

        Vector3 targetPosition = new Vector3(_target.position.x + _width, 2.0f, _target.position.z + _length);

        if (GetComponent<RectTransform>().position != targetPosition)
        {
            GetComponent<RectTransform>().position = targetPosition;
        }

        if (_canvas != null)
        {
            if (Camera.main == null)
                return;

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
