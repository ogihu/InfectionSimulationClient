using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MPX_Clothing_Slot : MPX_Clothing_Panel , IBeginDragHandler, IEndDragHandler, IDragHandler
{
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Transform originalParent;
    public Vector2 originalPosition;
    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        originalPosition = rectTransform.anchoredPosition;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent; // 현재 부모 저장
        canvasGroup.blocksRaycasts = false; // 드롭이 가능하도록 Raycast 비활성화
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / CanvasScaleFactor(); // 드래그 위치 업데이트
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true; // Raycast 다시 활성화
        if (transform.parent == originalParent) // 부모가 변경되지 않았다면 원래 자리로
        {
            rectTransform.anchoredPosition = originalPosition;
        }
    }

    private float CanvasScaleFactor()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        return canvas ? canvas.scaleFactor : 1f;
    }
}

