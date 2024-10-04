using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public class InteractableObject : MonoBehaviour
{
    [SerializeField] ItemData _itemData;
    FloatingUI _interactionKey;
    public string _description;

    private void Awake()
    {
        _itemData = new ItemData(gameObject.name);
        _interactionKey = Managers.UI.CreateUI("InteractionKey", UIManager.CanvasType.World).GetComponent<FloatingUI>();
        _interactionKey.Init(transform, y : 0.4f, isStatic : true);
        InActiveKeyUI();
    }

    public void ActiveKeyUI()
    {
        _interactionKey.gameObject.SetActive(true);

        if (gameObject.name != "EMRPC")
            return;

        if (Managers.Scenario.CurrentScenarioInfo == null)
            return;

        if(Managers.Scenario.CurrentScenarioInfo.Action == "EMRWrite")
            _interactionKey.SetDescription("EMR 법정감염병 신고서 작성");
        else if (Managers.Scenario.CurrentScenarioInfo.Action == "EMRRead")
            _interactionKey.SetDescription("EMR 법정감염병 신고서 확인 및 신고 진행");
        else
            _interactionKey.SetDescription("");
    }

    public void InActiveKeyUI()
    {
        _interactionKey.gameObject.SetActive(false);
    }

    public void GetItem()
    {
        Managers.Item.GetItem(_itemData);
    }
}
