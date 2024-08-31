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
                scenarioInfo.Equipment = scenarioList.Equipment;
                scenarioInfo.Action = scenarioList.Action;
                scenarioInfo.Hint = scenarioList.Hint;
                scenarioInfo.Confirm = scenarioList.Confirm;

                if (scenarioList.Keywords != null)
                {
                    string[] keywordSplit = scenarioList.Keywords.Split(',');
                    foreach (string keyword in keywordSplit)
                    {
                        scenarioInfo.Keywords.Add(keyword);
                    }
                }

                string newSpeech = scenarioList.DetailHint;
                string[] split = newSpeech.Split('/');
                scenarioInfo.DetailHint = split[0];
                foreach (var keyword in scenarioInfo.Keywords)
                {
                    if (split[1].Contains(keyword))
                    {
                        split[1] = split[1].Replace(keyword, $"<color=#00ff00>{keyword}</color>");
                    }
                }
                scenarioInfo.DetailHint += split[1];

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
