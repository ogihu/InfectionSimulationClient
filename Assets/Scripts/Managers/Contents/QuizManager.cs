using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class QuizManager
{
    GameObject quizUI;
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

        quizUI = Managers.UI.CreateUI("QUIZUI");
        if (quizUI == null)
            Debug.Log("퀴즈 UI를 찾을 수 없습니다");
        if (quizUI_Answer == null)
            init();
        Util.FindChildByName(quizUI, "Quest").GetComponent<TMP_Text>().text = Managers.Scenario.CurrentScenarioInfo.Question;
        for (int i = 0; i < 4;i++)
        {
            if(Managers.Scenario.CurrentScenarioInfo.Answers[i] == Managers.Scenario.CurrentScenarioInfo.Targets[0])
            {
                quizUI_Answer.transform.GetChild(i).gameObject.AddComponent<QuizAnswerButton>();
                continue;
            }           
            quizUI_Answer.transform.GetChild(i).GetChild(0).GetComponent<TMP_Text>().text = Managers.Scenario.CurrentScenarioInfo.Answers[i];
            quizUI_Answer.transform.GetChild(i).gameObject.AddComponent<QuizWorongButton>();
        }
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        DisablePlayerControls();

        yield return Managers.Instance.StartCoroutine(QuizCheck());
    }
    IEnumerator ShowQuizWithCountdown()
    {
        Managers.UI.CreateSystemPopup("PopupNotice", "3초 후에 돌발퀴즈가 발생합니다."); // 퀴즈 안내 문구
        if (TopRightUI == null)
            yield return null;

       yield return new WaitForSeconds(1f); // 잠깐 보여줌

        // 카운트다운 시작
        for (int i = 3; i > 0; i--)
        {
            TopRightUI = Managers.UI.CreateSystemPopup("PopupNotice", i.ToString()+ "초 후에 돌발퀴즈가 발생합니다.");
            //TopRightUI.GetComponent<TMP_Text>().text = i.ToString(); // 카운트다운 표시
            yield return new WaitForSeconds(1f); // 1초 대기
            Managers.UI.DestroyUI(TopRightUI);
        }

        // 카운트다운 후 퀴즈 표시
        Managers.UI.DestroyUI(TopRightUI);
    }
    IEnumerator QuizCheck()
    {
        while (!Passcheck)
        {
            yield return null;
        }
    }
    void DisablePlayerControls()
    {
        // 플레player이어 움직임을 제어하는 스크립트 비활성화
        if (Managers.Object.MyPlayer!= null)
        {
            // 예시: PlayerController라는 플레이어 제어 스크립트가 있다고 가정
            Managers.Object.MyPlayer.GetComponent<PlayerController>().enabled = false;
        }
    }
    float timer;
    GameObject RightPanel;  
    GameObject WrongPanel;
    public void AnswerButtonCheck()
    {
        if (RightPanel == null)
            RightPanel = Util.FindChildByName(quizUI, "RightPanel");
        RightPanel.SetActive(true);
        Managers.UI.CreateSystemPopup("WarningPopup", "정답입니다.");
        while(timer <1)
        {
            timer += Time.deltaTime;
        }
        timer = 0;
        Passcheck = true;
        NormalizationestroyUI();
    }
    
    public void WrongButton()
    {
        if (WrongPanel == null)
            WrongPanel = Util.FindChildByName(quizUI, "WrongPanel");
        WrongPanel.SetActive(true);
        Managers.UI.CreateSystemPopup("WarningPopup", "정답이 아닙니다.");
        while (timer < 1)
        {
            timer += Time.deltaTime;
        }
        timer = 0;
        WrongPanel.SetActive(false);
    }
    //정상화
    void NormalizationestroyUI()
    {
        Managers.Object.MyPlayer.GetComponent<PlayerController>().enabled = true;
        Cursor.lockState = CursorLockMode.Locked;  // 마우스 포인터를 고정 (화면 중앙에 위치하고 움직이지 않음)
        Cursor.visible = false;  // 마우스 포인터를 숨김
        Managers.UI.DestroyUI(quizUI);
    }
}
