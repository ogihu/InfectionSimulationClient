using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GUIKeywordPanel : MonoBehaviour
{
    //상단 문장과 하단 키워드 구간을 나누는 큰 UI
    GameObject _sentencesArea;
    GameObject _keywordsArea;

    //문장 컨테이너, 문자열UI, 빈칸UI, 키워드UI 프리팹
    GameObject _sentenceAreaPrefab;
    GameObject _sentenceUIPrefab;
    GameObject _blankUIPrefab;
    GameObject _keywordUIprefab;

    //생성된 문장 컨테이너, 문자열UI, 빈칸UI, 키워드UI 관리하기 위한 리스트
    List<GameObject> _sentenceAreas = new List<GameObject>();
    List<SentenceUI> _sentenceUIs = new List<SentenceUI>();
    List<BlankUI> _blankUIs = new List<BlankUI>();
    List<KeywordUI> _keywordUIs = new List<KeywordUI>();

    private void Awake()
    {
        Init();
    }

    public void Init()
    {
        _sentencesArea = Util.FindChildByName(gameObject, "SentencesArea");
        _sentenceAreaPrefab = Managers.Resource.Load<GameObject>("UI/SentenceArea");
        _sentenceUIPrefab = Managers.Resource.Load<GameObject>("UI/SentenceUI");
        _blankUIPrefab = Managers.Resource.Load<GameObject>("UI/BlankUI");

        _keywordsArea = Util.FindChildByName(gameObject, "KeywordsArea");
        _keywordUIprefab = Managers.Resource.Load<GameObject>("UI/KeywordUI");
    }

    void UpdateUI()
    {
        if (Managers.Scenario.CurrentScenarioInfo == null)
            return;

        string content = Managers.Scenario.CurrentScenarioInfo.Sentence;
        //string[] splits = content.Split('\n');
        string sentence = null;
        foreach(var ch in content)
        {

        }
    }

    /// <summary>
    /// 모든 공간, 문장, 빈칸, 키워드를 초기화
    /// </summary>
    void ResetSentences()
    {
        if(_keywordUIs.Count > 0)
        {
            foreach (var ui in _keywordUIs)
            {
                ui.GetComponentInChildren<TMP_Text>().text = null;
                Managers.UI.DestroyUI(ui.gameObject);
            }
            _keywordUIs.Clear();
        }

        if(_blankUIs.Count > 0)
        {
            foreach (var ui in _blankUIs)
            {
                ui.GetComponentInChildren<TMP_Text>().text = null;
                Managers.UI.DestroyUI(ui.gameObject);
            }
            _blankUIs.Clear();
        }

        if(_sentenceUIs.Count > 0)
        {
            foreach (var ui in _sentenceUIs)
            {
                ui.GetComponentInChildren<TMP_Text>().text = null;
                Managers.UI.DestroyUI(ui.gameObject);
            }
            _sentenceUIs.Clear();
        }

        if(_sentenceAreas.Count > 0)
        {
            foreach (var ui in _sentenceAreas)
            {
                Managers.UI.DestroyUI(ui.gameObject);
            }
            _sentenceAreas.Clear();
        }
    }

    /// <summary>
    /// 문자열UI와 빈칸UI가 저장될 공간 UI 생성
    /// </summary>
    /// <returns></returns>
    GameObject NewSentenceArea()
    {
        GameObject go = Managers.UI.CreateUI(_sentenceAreaPrefab, _sentencesArea.transform);
        _sentenceAreas.Add(go);
        return go;
    }

    /// <summary>
    /// 문장 내의 문자열UI 생성 및 텍스트 설정
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    GameObject NewSentenceUI(string content)
    {
        GameObject go = Managers.UI.CreateUI(_sentenceUIPrefab, _sentenceAreas[_sentenceAreas.Count - 1].transform);
        SetText(go, content);
        _sentenceUIs.Add(go.GetComponent<SentenceUI>());
        return go;
    }

    /// <summary>
    /// 문장 내의 빈칸UI 생성 및 정답 설정
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    GameObject NewBlankUI(string content)
    {
        GameObject go = Managers.UI.CreateUI(_blankUIPrefab, _sentenceAreas[_sentenceAreas.Count - 1].transform);
        SetText(go, content);
        _blankUIs.Add(go.GetComponent<BlankUI>());
        return go;
    }

    /// <summary>
    /// 하단의 키워드UI를 생성 및 텍스트 설정
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    GameObject NewKeywordUI(string content)
    {
        GameObject go = Managers.UI.CreateUI(_keywordUIprefab, _keywordsArea.transform);
        SetText(go, content);
        _keywordUIs.Add(go.GetComponent<KeywordUI>());
        return go;
    }

    /// <summary>
    /// 문장, 빈칸, 키워드 내의 텍스트를 설정
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="content"></param>
    void SetText(GameObject obj, string content)
    {
        obj.GetComponentInChildren<TMP_Text>().text = content;
    }
}
