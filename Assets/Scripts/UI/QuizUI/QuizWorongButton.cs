using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuizWorongButton : ButtonUI
{
    protected override void OnClicked()
    {
        base.OnClicked();
        //Managers.Instance.StartCoroutine(Managers.Quiz.WrongButton());
    }
}
