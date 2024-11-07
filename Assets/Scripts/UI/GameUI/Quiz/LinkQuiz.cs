using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinkQuiz : Quiz
{
    List<string> _answerList = new List<string>();

    public void AddAnswer(string answer)
    {
        _answerList.Add(answer);

        if (CheckAnswers())
        {
            StartCoroutine(CoOnRight());
        }
    }

    public void RemoveAnswer(string answer)
    {
        _answerList.Remove(answer);
    }

    bool CheckAnswers()
    {
        if (_answerList.Count < 4)
            return false;

        for(int i = 0; i < _answerList.Count; i++)
        {
            if (_answerList[i] != Managers.Scenario.CurrentScenarioInfo.Targets[i])
            {
                StartCoroutine(CoOnFault());
                return false;
            }
        }

        return true;
    }

    public override void OnFault()
    {
        base.OnFault();
        
        foreach(var button in _buttons)
        {
            button.AlreadyPressed = false;
            button.ChangePressedState(button.AlreadyPressed);
        }
        
        _answerList.Clear();
    }
}
