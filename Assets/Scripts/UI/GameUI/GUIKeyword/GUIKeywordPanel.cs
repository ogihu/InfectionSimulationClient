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

    bool _isInit = false;

    private void Awake()
    {
        Init();
    }

    public void Init()
    {
        _sentencesArea = Util.FindChildByName(gameObject, "SentencesArea");
        _sentenceAreaPrefab = Managers.Resource.Load<GameObject>("Prefabs/UI/GUIKeyword/SentenceArea");
        _sentenceUIPrefab = Managers.Resource.Load<GameObject>("Prefabs/UI/GUIKeyword/SentenceUI");
        _blankUIPrefab = Managers.Resource.Load<GameObject>("Prefabs/UI/GUIKeyword/BlankUI");

        _keywordsArea = Util.FindChildByName(gameObject, "KeywordsArea");
        _keywordUIprefab = Managers.Resource.Load<GameObject>("Prefabs/UI/GUIKeyword/KeywordUI");

        _isInit = true;
    }

    /// <summary>
    /// 남은 키워드가 있으면 false, 아니면 true
    /// </summary>
    /// <returns></returns>
    public bool CheckRemainKeywords()
    {
        if (_keywordsArea.transform.childCount > 0)
            return false;
        else
            return true;
    }

    /// <summary>
    /// UI들을 업데이트
    /// </summary>
    public void UpdateUI()
    {
        if (Managers.Scenario.CurrentScenarioInfo == null)
            return;

        if (_isInit == false)
            Init();

        string content = Managers.Scenario.CurrentScenarioInfo.Sentence;
        NewSentenceArea();

        string sentence = null;
        foreach(var ch in content)
        {
            if(ch == '[')
            {
                NewSentenceUI(sentence);
                sentence = null;
                continue;
            }
            else if(ch == ']')
            {
                NewBlankUI(sentence);
                NewKeywordUI(sentence);
                sentence = null;
                continue;
            }
            else if(ch == '\n')
            {
                NewSentenceUI(sentence);
                sentence = null;
                NewSentenceArea();
            }
            else
            {
                sentence += ch;
            }
        }
        
        if (!string.IsNullOrEmpty(sentence))
        {
            NewSentenceUI(sentence);
        }

        RandomizeKeywords();
    }

    public void RandomizeKeywords()
    {
        // 현재 게임 오브젝트의 자식 개수 가져오기
        int childCount = _keywordsArea.transform.childCount;

        // 자식 오브젝트의 인덱스를 담을 배열 생성
        int[] indices = new int[childCount];
        for (int i = 0; i < childCount; i++)
        {
            indices[i] = i; // 인덱스 초기화
        }

        // 인덱스 배열을 랜덤하게 섞기
        for (int i = 0; i < childCount; i++)
        {
            int randomIndex = Random.Range(i, childCount);
            // 인덱스 교환
            int temp = indices[i];
            indices[i] = indices[randomIndex];
            indices[randomIndex] = temp;
        }

        // 랜덤 인덱스에 따라 자식 오브젝트 순서 변경
        for (int i = 0; i < childCount; i++)
        {
            _keywordsArea.transform.GetChild(indices[i]).SetSiblingIndex(i);
        }
    }

    /// <summary>
    /// 모든 공간, 문장, 빈칸, 키워드를 초기화
    /// </summary>
    public void ResetUIs()
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
        SetText(go, "빈칸");
        _blankUIs.Add(go.GetComponent<BlankUI>());
        go.GetComponent<BlankUI>().Answer = content;
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
    public static void SetText(GameObject obj, string content)
    {
        obj.GetComponentInChildren<TMP_Text>().text = content;
    }
}
