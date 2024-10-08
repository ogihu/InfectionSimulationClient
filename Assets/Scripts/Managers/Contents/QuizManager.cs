using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class QuizManager
{
    GameObject quizUI;
    bool Passcheck = false;

    public IEnumerator QuizUI(int Quiznumber)
    {
        Managers.Scenario.ScenarioAssist.transform.GetChild(0).GetComponent<TMP_Text>().text = "퀴즈";
        quizUI = Managers.UI.CreateUI("QUIZUI" + Quiznumber);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        DisablePlayerControls();

        yield return Managers.Instance.StartCoroutine(QuizCheck());


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
    
    public void AnswerButtonCheck()
    {
        Managers.UI.CreateSystemPopup("WarningPopup", "정답입니다.");
        Passcheck = true;
        NormalizationestroyUI();
    }
    
    public void WrongButton()
    {
        Managers.UI.CreateSystemPopup("WarningPopup", "정답이 아닙니다.");
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
