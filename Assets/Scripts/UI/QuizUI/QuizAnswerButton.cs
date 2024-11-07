using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuizAnswerButton : ButtonUI
{
    protected override void OnClicked()
    {
        base.OnClicked();
        //Managers.Instance.StartCoroutine(Managers.Quiz.AnswerButtonCheck());
    }
}
