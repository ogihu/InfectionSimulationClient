using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class QuizManager
{
    public enum QuizType
    {
        Normal,
        Link
    }

    public Quiz QuizUI;
    GameObject popup;
    WaitForSeconds _wait1sec = new WaitForSeconds(1);
    public GameObject MPX_Clothing_Panel;
    public bool MPX_Clothing_Panel_opencheck = false;

    public IEnumerator CoQuizCount(int count, QuizType type = QuizType.Normal)
    {
        popup = Managers.UI.CreateUI("PopupNotice"); // 퀴즈 안내 문구

        for (int i = count; i > 0; i--)
        {
            popup.transform.GetChild(0).GetComponent<TMP_Text>().alignment = TextAlignmentOptions.Center;   //중앙정렬
            popup.transform.GetChild(0).GetComponent<TMP_Text>().text = i.ToString() + "초 뒤에 돌발 퀴즈가 주어집니다.";
            yield return _wait1sec;
        }

        Managers.UI.DestroyUI(popup);

        if(type == QuizType.Normal)
        {
            QuizUI = Managers.UI.CreateUI("Quiz").GetComponent<Quiz>();
        }
        else
        {
            QuizUI = Managers.UI.CreateUI("LinkQuiz").GetComponent<Quiz>();
        }
        QuizUI.SetQuestText(Managers.Scenario.CurrentScenarioInfo.Question);
        QuizUI.SetButtonText(Managers.Scenario.CurrentScenarioInfo.Answers);
    }

    public void Clear()
    {
        if (QuizUI != null)
            Managers.Resource.Destroy(QuizUI.gameObject);

        if (popup != null)
            Managers.Resource.Destroy(popup);
    }
}
