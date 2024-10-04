using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlankUI : MonoBehaviour
{
    public Image targetImage;
    private Color originalColor;
    public string Answer { get; set; }
    Coroutine _co;

    void Awake()
    {
        targetImage = GetComponent<Image>();

        // 이미지의 원래 색상 저장
        if (targetImage != null)
        {
            originalColor = targetImage.color;
        }
    }

    public void Recalculate()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
    }

    // 색상을 1초 동안 빨간색으로 변경한 후 다시 원래 색상으로 돌리는 함수
    public void ChangeColorWarning()
    {
        _co = StartCoroutine(ChangeColorCoroutine());
    }

    // 코루틴으로 색상 변경
    private IEnumerator ChangeColorCoroutine()
    {
        if (targetImage != null)
        {
            // 색상을 빨간색으로 변경
            targetImage.color = Color.red;
            // 1초 대기
            yield return new WaitForSeconds(1.0f);
            // 색상을 원래 색상으로 복구
            targetImage.color = originalColor;
        }
    }

    private void OnDisable()
    {
        _co = null;
        targetImage.color = originalColor;
    }
}
