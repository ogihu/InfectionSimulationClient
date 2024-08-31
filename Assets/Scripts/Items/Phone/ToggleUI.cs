using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ToggleUI : MonoBehaviour
{
    Action<bool> _action;

    TMP_Text _label;
    public TMP_Text Label
    {
        get 
        { 
            if(_label == null)
                _label = Util.FindChildByName(gameObject, "Label").GetComponent<TMP_Text>();

            return _label; 
        }
    }

    protected virtual void Awake()
    {
        _label = Util.FindChildByName(gameObject, "Label").GetComponent<TMP_Text>();
        GetComponent<Toggle>().onValueChanged.AddListener(OnValueChanged);
    }

    protected virtual void OnValueChanged(bool isOn)
    {
        if (isOn)
        {
            if(Managers.Scenario.CurrentScenarioInfo != null)
            {
                if (Managers.Scenario.CurrentScenarioInfo.Targets.Contains(_label.text))
                {
                    _label.color = Color.green;
                }
                else
                {
                    _label.color = Color.red;
                }
            }
        }
        else
        {
            if(_label.color != Color.white)
                _label.color = Color.white;
        }

        if (_action != null)
            _action.Invoke(isOn);

        //버튼 눌렀을 때 필요한 이벤트(ex. 사운드)
    }

    public virtual void SetEvent(Action<bool> action)
    {
        _action = action;
    }

    public void SetLabel(string text)
    {
        Label.text = text;
    }

    private void OnDestroy()
    {
        GetComponent<Toggle>().onValueChanged.RemoveListener(OnValueChanged);
    }
}
