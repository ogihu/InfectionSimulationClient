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

    private GameObject getItemUIInstance;

    private void Awake()
    {
        _itemData = new ItemData(gameObject.name);
        _interactionKey = Managers.UI.CreateUI("InteractionKey", UIManager.CanvasType.World).GetComponent<FloatingUI>();
        _interactionKey.Init(transform, y: 0.4f, isStatic: true);
        InActiveKeyUI();
    }

    public void ActiveKeyUI()
    {
        _interactionKey.gameObject.SetActive(true);

        if (gameObject.name != "EMRPC")
            return;

        if (Managers.Scenario.CurrentScenarioInfo == null)
            return;

        if (Managers.Scenario.CurrentScenarioInfo.Action == "EMRWrite")
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

        ShowItemSpriteOnScreen(_itemData.Icon);
    }

    private void ShowItemSpriteOnScreen(Sprite itemSprite)
    {
        if (itemSprite == null)
        {
            Debug.LogError("Item sprite is null.");
            return;
        }

        GameObject spriteDisplay = new GameObject("ItemSpriteDisplay");
        spriteDisplay.transform.SetParent(GameObject.Find("Canvas").transform, false);  

        Image displayImage = spriteDisplay.AddComponent<Image>();
        displayImage.sprite = itemSprite;

        RectTransform rectTransform = displayImage.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = Vector2.zero;  
        rectTransform.sizeDelta = new Vector2(500, 500);  

        StartCoroutine(ShrinkAndMoveToBottom(spriteDisplay, rectTransform));
    }

    private IEnumerator ShrinkAndMoveToBottom(GameObject spriteDisplay, RectTransform rectTransform)
    {
        float duration = 0.95f; 
        float elapsedTime = 0f;

        Vector2 initialSize = new Vector2(100, 100);  
        Vector2 targetSize = new Vector2(500, 500);  

        Vector2 initialPosition = rectTransform.anchoredPosition;
        Vector2 targetPosition = new Vector2(0, -Screen.height);  

        rectTransform.sizeDelta = initialSize;  

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;

            rectTransform.sizeDelta = Vector2.Lerp(initialSize, targetSize, t);

            rectTransform.anchoredPosition = Vector2.Lerp(initialPosition, targetPosition, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        rectTransform.sizeDelta = targetSize;
        rectTransform.anchoredPosition = targetPosition;
        Destroy(spriteDisplay);
    }
        

}



