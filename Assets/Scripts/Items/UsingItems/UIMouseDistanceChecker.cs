using UnityEngine.EventSystems;
using UnityEngine;

public class UIMouseDistanceChecker : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public LesionUI LesionUI;
    public bool AlreadyTaked = false;

    private bool isPointerInside = false;
    private Vector2 lastMousePosition;  // 이전 마우스 위치

    void Update()
    {
        if (isPointerInside)
        {
            // 현재 마우스 위치 가져오기
            Vector2 currentMousePosition = Input.mousePosition;

            // 이전 위치와 현재 위치 간의 거리 계산 및 누적
            LesionUI.distanceSum += Vector2.Distance(lastMousePosition, currentMousePosition);
            lastMousePosition = currentMousePosition;

            // 누적 거리가 checkDistance를 초과 했을 때
            if (LesionUI.distanceSum >= LesionUI.checkDistance)
            {
                Debug.Log("Total distance moved on UI reached: " + LesionUI.distanceSum);
                Complete();
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // 마우스가 UI에 들어왔을 때 위치 기록
        lastMousePosition = eventData.position;
        isPointerInside = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // 마우스가 UI를 벗어났을 때 리셋
        isPointerInside = false;
        LesionUI.distanceSum = 0f;
    }

    void Complete()
    {
        LesionUI.distanceSum = 0f;
        
        // 이미 채취한 병변일 경우 경고, 초기화
        if (AlreadyTaked)
        {
            Managers.UI.CreateSystemPopup("WarningPopup", $"이미 검체를 채취한 병변입니다.", UIManager.NoticeType.Warning);
        }
        // 아닐 경우 통과
        else
        {
            LesionUI.CompleteLesion(this);
        }
    }
}