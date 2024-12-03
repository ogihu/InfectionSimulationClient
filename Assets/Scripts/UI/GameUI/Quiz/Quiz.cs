using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Quiz : MonoBehaviour
{
    protected TMP_Text _quest;
    protected List<AnswerButton> _buttons = new List<AnswerButton>();
    protected GameObject _wrongPanel;
    protected GameObject _rightPanel;
    protected WaitForSeconds _wait2Sec;
    GameObject _textArea;

    /// <summary>
    /// true일 때만 AnswerButton 선택 가능
    /// </summary>
    public bool CanSelectAnswer
    {
        get
        {
            //정답 패널이나 오답 패널이 활성화 되어있으면 정답 버튼 선택 불가
            if (_wrongPanel.activeSelf || _rightPanel.activeSelf)
                return false;
            else
                return true;
        }
    }

    protected void Awake()
    {
        _quest = Util.FindChildByName(gameObject, "Quest").GetComponent<TMP_Text>();
        
        for(int i = 1; i <= 4; i++)
        {
            GameObject button = Util.FindChildByName(gameObject, $"AnswerButton{i}");

            if (button.GetComponent<AnswerButton>() == null)
                button.AddComponent<AnswerButton>();

            _buttons.Add(button.GetComponent<AnswerButton>());           
        }

        _wrongPanel = Util.FindChildByName(gameObject, "WrongPanel");
        _rightPanel = Util.FindChildByName(gameObject, "RightPanel");
        _textArea = Util.FindChildByName(_rightPanel, "TextArea");
        _wait2Sec = new WaitForSeconds(2f);

        Init();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void SetQuestText(string content)
    {
        _quest.text = content;
    }

    public void SetButtonText(List<string> contents)
    {
        if(contents.Count != 4)
        {
            Debug.LogError("퀴즈 보기의 갯수가 4개가 아닙니다.");
            return;
        }

        for(int i = 0; i < contents.Count; i++)
        {
            _buttons[i].GetComponentInChildren<TMP_Text>().text = contents[i];
        }
    }

    public IEnumerator CoOnFault()
    {
        _wrongPanel.SetActive(true);

        yield return _wait2Sec;

        _wrongPanel.SetActive(false);
        OnFault();
    }

    public IEnumerator CoOnRight()
    {
        _rightPanel.SetActive(true);

        if (string.IsNullOrEmpty(Managers.Scenario.CurrentScenarioInfo.QuizDescription))
            _textArea.SetActive(false);
        else
        {
            _textArea.SetActive(true);
            _textArea.transform.GetComponentInChildren<TMP_Text>().text = Managers.Scenario.CurrentScenarioInfo.QuizDescription;
        }

        yield return _wait2Sec;

        _rightPanel.SetActive(false);
        OnRight();
    }

    public virtual void OnFault()
    {

    }

    public virtual void OnRight()
    {
        //시나리오 통과 조건 입력
        Managers.Scenario.MyAction = Managers.Scenario.CurrentScenarioInfo.Action;
        Managers.Scenario.Targets = Managers.Scenario.CurrentScenarioInfo.Targets;

        Cursor.lockState = CursorLockMode.Locked;  // 마우스 포인터를 고정 (화면 중앙에 위치하고 움직이지 않음)
        Cursor.visible = false;  // 마우스 포인터를 숨김

        Managers.UI.DestroyUI(gameObject);
    }

    public void Init()
    {
        _wrongPanel.SetActive(false);
        _rightPanel.SetActive(false);
    }
}
