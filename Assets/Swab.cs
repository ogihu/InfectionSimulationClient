using Ricimi;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Swab : UsingItem
{
    bool check = false;
    public Texture2D cursorTexture;  // 사용할 커서 이미지
    public Vector2 hotSpot = Vector2.zero; // 커서의 포인터 위치 설정 (0,0은 좌상단)
    public float requiredRubTime = 2.0f; // 마우스로 문질러야 할 시간
    private float rubTimer = 0f;         // 문지른 시간
    private bool isRubbing = false;      // 문지름 상태 확인

    private GraphicRaycaster raycaster;  // UI 요소의 Raycast 감지를 위한 변수
    private EventSystem eventSystem;     // EventSystem 변수
    GameObject popup;
    GameObject lesion;
    private void Update()
    {
        
    }
    public override bool Use(BaseController character)
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        // 필요한 컴포넌트 가져오기
        raycaster = FindObjectOfType<GraphicRaycaster>(); // Canvas에 있는 GraphicRaycaster를 찾습니다.
        eventSystem = EventSystem.current;
        cursorTexture = Resources.Load<Texture2D>("Use_Swab");
        StartCoroutine(Use_Checking());

        return base.Use(character);
    }
    IEnumerator Use_Checking()
    {
        
        int i = 0;
        popup = Managers.UI.CreateUI("PopupNotice");
        // 카운트다운 시작
        for (i = 3; i > 0; i--)
        {
            if (popup == null)
                yield return null;
            popup.transform.GetChild(0).GetComponent<TMP_Text>().alignment = TextAlignmentOptions.Center;   //중앙정렬
            popup.transform.GetChild(0).GetComponent<TMP_Text>().text = i.ToString() + "초 뒤에 병변 이미지가 주어집니다.";
            yield return new WaitForSeconds(1f); // 1초 대기
        }
        Managers.UI.DestroyUI(popup);

        lesion = Managers.UI.CreateUI("Lesion");
        Cursor.SetCursor(cursorTexture, hotSpot, CursorMode.Auto);
        while (!check)
        {
            // 마우스가 눌려져 있는 동안 매 프레임마다 체크
            if (Input.GetMouseButton(0)) // 0은 마우스 왼쪽 버튼
            {
                // UI 위에 있는지 확인
                if (IsPointerOverSpecificUI())
                {
                    // 문지름을 시작했음을 표시하고, 타이머 시작
                    isRubbing = true;
                    rubTimer += Time.deltaTime;

                    // 타이머가 필요한 시간을 초과하면 코루틴 종료 또는 작업 실행
                    if (rubTimer >= requiredRubTime)
                    {
                        StartCoroutine(OnRubComplete());
                        ResetRubTimer(); // 타이머 초기화
                    }
                }
                else
                {
                    ResetRubTimer(); // UI 요소 밖으로 벗어날 경우 타이머 초기화
                }
            }
            else
            {
                ResetRubTimer(); // 마우스 버튼에서 손을 떼면 타이머 초기화
            }

            yield return null;
        }
        yield return StartCoroutine(Checking());
    }

    private bool IsPointerOverSpecificUI(string uiName = "Right_Cricle")
    {
        PointerEventData pointerEventData = new PointerEventData(eventSystem)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        raycaster.Raycast(pointerEventData, results);

        // Raycast 결과에서 특정 이름을 가진 UI가 있는지 확인
        foreach (RaycastResult result in results)
        {
            if (result.gameObject.name == uiName)
            {
                return true;
            }
        }

        return false; // 목표 UI가 없으면 false 반환
    }

    private void ResetRubTimer()
    {
        isRubbing = false;
        rubTimer = 0f;
    }

    IEnumerator OnRubComplete()
    {
        Debug.Log("문지름 완료!");
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        check = true; // 문지름 완료 후 check를 true로 설정
        Destroy(lesion);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        yield return null; // 문지름 완료 시 수행할 작업 추가 가능
    }
    IEnumerator Checking()
    {
        while(!check)
            yield return null;
    }
}
