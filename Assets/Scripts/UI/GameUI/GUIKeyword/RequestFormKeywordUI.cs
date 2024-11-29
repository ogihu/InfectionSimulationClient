using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class RequestFormKeywordUI : KeywordUI
{
    MpoxRequestForm _form;

    protected override void Awake()
    {
        base.Awake();

    }

    public override void OnEndDrag(PointerEventData eventData)
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
                if (result.gameObject.GetComponent<BlankUI>().Answer == this.GetComponentInChildren<TMP_Text>().text)
                {
                    GUIKeywordPanel.SetText(result.gameObject, this.GetComponentInChildren<TMP_Text>().text);
                    result.gameObject.GetComponent<BlankUI>().Recalculate();
                    this.transform.SetParent(result.gameObject.transform);
                    _isSame = true;
                    _rect.localPosition = Vector2.zero;

                    if (_form == null)
                        _form = Util.FindParentByName(gameObject, "MpoxRequestForm").GetComponent<MpoxRequestForm>();

                    _form.CheckRight();
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
    }
}
