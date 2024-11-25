using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonUI : MonoBehaviour
{
    Action _action;
    protected Button _button;

    protected virtual void Awake()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(OnClicked);
    }

    protected virtual void OnClicked()
    {
        if(_action != null)
            _action.Invoke();

        Managers.Sound.Play("ButtonClick");
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
