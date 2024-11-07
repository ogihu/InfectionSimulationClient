using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AnswerButton : ButtonUI
{
    public bool AlreadyPressed = false;
    private Color _normalColor;
    private Color _selectedColor = new Color32(0x9D, 0x9D, 0x9D, 0xFF);

    protected override void Awake()
    {
        base.Awake();

        _normalColor = _button.image.color;
    }

    protected override void OnClicked()
    {
        if (!Managers.Quiz.QuizUI.CanSelectAnswer)
            return;

        base.OnClicked();

        //보기를 순서대로 선택하는 퀴즈일 경우
        if(Managers.Quiz.QuizUI is LinkQuiz)
        {
            if (!AlreadyPressed)
            {
                Managers.Quiz.QuizUI.GetComponent<LinkQuiz>().AddAnswer(GetComponentInChildren<TMP_Text>().text);
                AlreadyPressed = true;
            }
            else
            {
                Managers.Quiz.QuizUI.GetComponent<LinkQuiz>().RemoveAnswer(GetComponentInChildren<TMP_Text>().text);
                AlreadyPressed = false;
            }
            ChangePressedState(AlreadyPressed);
        }
        //일반 퀴즈일 경우
        else
        {
            if (Managers.Scenario.CurrentScenarioInfo.Targets[0] == GetComponentInChildren<TMP_Text>().text)
            {
                Managers.Quiz.QuizUI.StartCoroutine(Managers.Quiz.QuizUI.CoOnRight());
            }
            else
            {
                Managers.Quiz.QuizUI.StartCoroutine(Managers.Quiz.QuizUI.CoOnFault());
            }
        }
    }

    public void ChangePressedState(bool pressed)
    {
        if (pressed)
        {
            _button.image.color = _selectedColor;
        }
        else
        {
            _button.image.color = _normalColor;
        }
    }
}
