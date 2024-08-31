using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonUI : MonoBehaviour
{
    Action _action;

    protected virtual void Awake()
    {
        GetComponent<Button>().onClick.AddListener(OnClicked);
    }

    protected virtual void OnClicked()
    {
        if(_action != null)
            _action.Invoke();
        //버튼 눌렀을 때 필요한 이벤트(ex. 사운드)
    }

    public virtual void SetEvent(Action action)
    {
        _action = action;
    }

    private void OnDestroy()
    {
        GetComponent<Button>().onClick.RemoveListener(OnClicked);
    }
}
