using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class KeywordUI : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    LayoutElement _layoutElement;
    RectTransform _rect;
    bool _isSame;

    void Awake()
    {
        _layoutElement = GetComponent<LayoutElement>();
        _rect = GetComponent<RectTransform>();
        _isSame = false;
    }

    void OnDisable()
    {
        _layoutElement.ignoreLayout = false;
        _isSame = false;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_isSame)
            return;

        _layoutElement.ignoreLayout = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_isSame)
            return;

        _rect.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (_isSame)
            return;

        // Raycast를 통해 현재 마우스 위치에 있는 UI 오브젝트를 찾음
        PointerEventData pointerData = new PointerEventData(EventSystem.current);
        pointerData.position = eventData.position;

        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, raycastResults);

        // Raycast 결과 중에 BlankUI 이름을 가진 오브젝트가 있는지 확인
        foreach (var result in raycastResults)
        {
            if (result.gameObject.name == "BlankUI")
            {
                // BlankUI 오브젝트의 자식으로 드래그한 오브젝트를 이동\
                if(result.gameObject.GetComponent<BlankUI>().Answer == this.GetComponentInChildren<TMP_Text>().text)
                {
                    GUIKeywordPanel.SetText(result.gameObject, this.GetComponentInChildren<TMP_Text>().text);
                    result.gameObject.GetComponent<BlankUI>().Recalculate();
                    this.transform.SetParent(result.gameObject.transform);
                    _isSame = true;
                    _rect.localPosition = Vector2.zero;
                }
                else
                {
                    result.gameObject.GetComponent<BlankUI>().ChangeColorWarning();
                }
                break;
            }
        }

        if (_isSame == false)
            _layoutElement.ignoreLayout = false;

        Managers.Keyword.CheckRemainKeywords();
    }
}
