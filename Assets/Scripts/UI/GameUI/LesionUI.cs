using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LesionUI : MonoBehaviour
{
    public DrySwab Swab;
    public List<UIMouseDistanceChecker> Circles = new List<UIMouseDistanceChecker>();
    Image _lesionRatio;

    public readonly float checkDistance = 600f;        // 마우스가 이동해야 하는 총 거리
    public float distanceSum = 0f;           // 이동 거리의 누적 합

    private void Awake()
    {
        for(int i = 1; i <= 3; i++)
        {
            GameObject go = Util.FindChildByName(gameObject, $"LesionArea{i}");
            Circles.Add(go.GetComponent<UIMouseDistanceChecker>());
            Circles[i - 1].LesionUI = this;
            
            if (Managers.Scenario.TakedLesion.Contains(go.name))
            {
                Circles[i - 1].AlreadyTaked = true;
            }
        }
        _lesionRatio = Util.FindChildByName(gameObject, "LesionRatio").GetComponent<Image>();
    }

    private void Update()
    {
        _lesionRatio.fillAmount = distanceSum / checkDistance;
    }

    public void CompleteLesion(UIMouseDistanceChecker area)
    {
        if(Swab == null)
        {
            Debug.LogError("LesionUI에 DrySwab이 할당되지 않았습니다.");
            return;
        }

        Managers.Scenario.TakedLesion.Add(area.gameObject.name);
        Swab.CompleteLesion();
        Managers.UI.DestroyUI(gameObject);
    }
}
