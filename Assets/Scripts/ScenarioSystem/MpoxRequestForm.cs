using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MpoxRequestForm : MonoBehaviour
{
    int _rightCount;
    Transform _keywords;

    private void Awake()
    {
        _rightCount = 0;
    }

    private void Start()
    {
        ChangeKeywordsOrder();
    }

    public void ChangeKeywordsOrder()
    {
        if (_keywords == null)
        {
            GameObject keywordUIsObject = Util.FindChildByName(gameObject, "KeywordUIs");
            if (keywordUIsObject != null)
            {
                _keywords = keywordUIsObject.transform;
            }
            else
            {
                Debug.LogError("KeywordUIs not found under the current GameObject.");
                return; // 또는 적절한 예외 처리
            }
        }

        // 자식 Transform들을 리스트에 저장
        List<Transform> children = new List<Transform>();
        for (int i = 0; i < _keywords.childCount; i++)
        {
            children.Add(_keywords.GetChild(i));
        }

        // 자식 Transform들을 임시 부모로 이동
        GameObject tempParent = new GameObject("TempParent");
        foreach (Transform child in children)
        {
            child.SetParent(tempParent.transform);
        }

        // 리스트를 랜덤으로 섞기
        for (int i = 0; i < children.Count; i++)
        {
            Transform temp = children[i];
            int randomIndex = Random.Range(i, children.Count);
            children[i] = children[randomIndex];
            children[randomIndex] = temp;
        }

        // 섞인 순서대로 다시 원래 부모에 추가
        foreach (Transform child in children)
        {
            child.SetParent(_keywords);
        }

        // 임시 부모 객체 삭제
        Destroy(tempParent);
    }


    public void CheckRight()
    {
        _rightCount++;

        if(_rightCount >= 6)
        {
            GameObject effectUI = Managers.UI.CreateUI("EffectUI");
            Managers.EMR.CanClose = false;
            Managers.Instance.StartCoroutine(CoCloseAfterDelay(3f, effectUI));
        }
    }

    private IEnumerator CoCloseAfterDelay(float delay, GameObject effectUI)
    {
        yield return new WaitForSeconds(delay);

        Managers.EMR.CanClose = true;
        Managers.EMR.CloseEMR();
        Managers.Scenario.MyAction = "SCRFWrite";

        if (effectUI != null)
        {
            Managers.UI.DestroyUI(effectUI);
        }
    }
}