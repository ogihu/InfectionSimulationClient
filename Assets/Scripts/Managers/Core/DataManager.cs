using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Define;

public class DataManager
{
    string _jsonPath = "Data/ScenarioData";
    public Dictionary<string, Dictionary<int, ScenarioInfo>> ScenarioData = new Dictionary<string, Dictionary<int, ScenarioInfo>>();

    public void Init()
    {
        string jsonToText = Resources.Load<TextAsset>(_jsonPath).text;
        var scenarioData = JsonConvert.DeserializeObject<Dictionary<string, List<JScenarioInfo>>>(jsonToText);

        foreach(var scenarioDict in scenarioData)
        {
            Dictionary<int, ScenarioInfo> newDict = new Dictionary<int, ScenarioInfo>();
            foreach(var scenarioList in scenarioDict.Value)
            {
                ScenarioInfo scenarioInfo = new ScenarioInfo();
                scenarioInfo.Progress = scenarioList.Progress;
                scenarioInfo.Place = scenarioList.Place;
                scenarioInfo.Position = scenarioList.Position;
                scenarioInfo.Item = scenarioList.Item;
                scenarioInfo.Action = scenarioList.Action;
                scenarioInfo.Hint = scenarioList.Hint;
                scenarioInfo.Confirm = scenarioList.Confirm;
                scenarioInfo.Sentence = scenarioList.Sentence;
                scenarioInfo.Question = scenarioList.Question;

                if (!string.IsNullOrEmpty(scenarioList.Sentence))
                {
                    foreach (var ch in scenarioList.Sentence)
                    {
                        if (ch == '[' || ch == ']')
                            continue;

                        scenarioInfo.OriginalSentence += ch;
                    }
                }

                string guiWord = null;
                bool isKeyword = false;

                if (!string.IsNullOrEmpty(scenarioList.Sentence))
                {
                    foreach (var word in scenarioList.Sentence)
                    {
                        if (word == '[')
                        {
                            isKeyword = true;
                            continue;
                        }
                        else if (word == ']')
                        {
                            isKeyword = false;
                            scenarioInfo.GUIKeywords.Add(guiWord);
                            guiWord = null;
                            continue;
                        }

                        if (isKeyword)
                        {
                            guiWord += word;
                        }
                    }
                }

                if (scenarioList.STTKeywords != null)
                {
                    string[] keywordSplit = scenarioList.STTKeywords.Split(',');
                    foreach (string keyword in keywordSplit)
                    {
                        scenarioInfo.STTKeywords.Add(keyword);
                    }
                }

                if (scenarioList.Answers != null)
                {
                    string[] keywordSplit = scenarioList.Answers.Split(',');
                    foreach (string keyword in keywordSplit)
                    {
                        scenarioInfo.Answers.Add(keyword);
                    }
                }

                if (!string.IsNullOrEmpty(scenarioList.ObjectIndicator))
                {
                    string[] Split = scenarioList.ObjectIndicator.Split(',');
                    foreach (string obj in Split)
                    {
                        scenarioInfo.ObjectIndicator.Add(obj);
                    }
                }

                if (!string.IsNullOrEmpty(scenarioList.DetailHint))
                {
                    string newSpeech = scenarioList.DetailHint;
                    string[] split = newSpeech.Split('/');
                    scenarioInfo.DetailHint = split[0];
                    foreach (var keyword in scenarioInfo.STTKeywords)
                    {
                        if (split[1].Contains(keyword))
                        {
                            split[1] = split[1].Replace(keyword, $"<color=#00ff00>{keyword}</color>");
                        }
                    }

                    if(split.Length > 1 )
                        scenarioInfo.DetailHint += split[1];
                }

                if (scenarioList.Targets != null)
                {
                    string[] targetSplit = scenarioList.Targets.Split(',');
                    foreach (string target in targetSplit)
                    {
                        scenarioInfo.Targets.Add(target);
                    }
                }

                newDict.Add(scenarioList.Progress, scenarioInfo);
            }
            ScenarioData.Add(scenarioDict.Key, newDict);
        }
    }
}
