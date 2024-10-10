using Ricimi;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class QuizManager
{
    public GameObject quizUI;
    public GameObject popup;
    GameObject quizUI_Answer;
    GameObject TopRightUI;
    bool Passcheck = false;

    void init()
    {
        quizUI_Answer = Util.FindChildByName(quizUI, "Answer");
    }
    public IEnumerator QuizUI(int number)
    {
       
        
        Util.FindChildByName(Managers.Scenario.ScenarioAssist, "TurnNotice").GetComponent<TMP_Text>().text = "퀴즈";
        Managers.Instance.StartCoroutine(ShowQuizWithCountdown());
        
        yield return Managers.Instance.StartCoroutine(QuizCheck());
    }
    
    bool QuizApear = false;
    IEnumerator ShowQuizWithCountdown()
    {
        int i = 0;
        popup = Managers.UI.CreateUI("PopupNotice"); // 퀴즈 안내 문구
        popup.transform.GetChild(0).GetComponent<TMP_Text>().text = "돌발 퀴즈가 발생합니다.";
       yield return new WaitForSeconds(1f); // 잠깐 보여줌

        // 카운트다운 시작
        for (i = 3; i > 0; i--)
        {
            if (popup == null)
                yield return null;
            popup.transform.GetChild(0).GetComponent<TMP_Text>().text = i.ToString() + "초 후에 돌발퀴즈가 발생합니다.";
            yield return new WaitForSeconds(1f); // 1초 대기
        }
        Managers.UI.DestroyUI(popup);
        while(!QuizApear)
        {
            quizUI = Managers.UI.CreateUI("QUIZUI");
            QuizApear = true;
        }
        if (QuizApear)
        {
            if (quizUI == null)
                Debug.Log("퀴즈 UI를 찾을 수 없습니다");
            if (quizUI_Answer == null)
                init();
            Util.FindChildByName(quizUI, "Quest").GetComponent<TMP_Text>().text = Managers.Scenario.CurrentScenarioInfo.Question;
            for (i = 0; i < 4; i++)
            {
                if (Managers.Scenario.CurrentScenarioInfo.Answers[i] == Managers.Scenario.CurrentScenarioInfo.Targets[0])
                {
                    quizUI_Answer.transform.GetChild(i).gameObject.AddComponent<QuizAnswerButton>();
                    continue;
                }
                quizUI_Answer.transform.GetChild(i).GetChild(0).GetComponent<TMP_Text>().text = Managers.Scenario.CurrentScenarioInfo.Answers[i];
                quizUI_Answer.transform.GetChild(i).gameObject.AddComponent<QuizWorongButton>();
            }
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

    }
    IEnumerator QuizCheck()
    {
        while (!Passcheck)
        {
            yield return null;
        }
    }

    float timer;
    GameObject RightPanel;  
    GameObject WrongPanel;
    public IEnumerator AnswerButtonCheck()
    {
        timer = 0;
        if (RightPanel == null)
            RightPanel = Util.FindChildByName(quizUI, "RightPanel");
        RightPanel.SetActive(true);
        //Managers.UI.CreateSystemPopup("WarningPopup", "정답입니다.");
        yield return new WaitForSeconds(2f);

        Passcheck = true;
        NormalizationestroyUI();
    }

    public IEnumerator WrongButton()
    {
        timer = 0;
        if (WrongPanel == null)
            WrongPanel = Util.FindChildByName(quizUI, "WrongPanel");
        WrongPanel.SetActive(true);
        //Managers.UI.CreateSystemPopup("WarningPopup", "정답이 아닙니다.");
        yield return new WaitForSeconds(1f);

        WrongPanel.SetActive(false);
    }
    //정상화
    void NormalizationestroyUI()
    {
        Managers.Object.MyPlayer.GetComponent<MyPlayerController>().enabled = true;
        Cursor.lockState = CursorLockMode.Locked;  // 마우스 포인터를 고정 (화면 중앙에 위치하고 움직이지 않음)
        Cursor.visible = false;  // 마우스 포인터를 숨김
        Managers.UI.DestroyUI(quizUI);
        Managers.Scenario.CompleteCount++;
    }
}
