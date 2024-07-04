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
        var scenarioData = JsonConvert.DeserializeObject<Dictionary<string, List<ScenarioInfo>>>(jsonToText);

        foreach(var scenarioDict in scenarioData)
        {
            Dictionary<int, ScenarioInfo> newDict = new Dictionary<int, ScenarioInfo>();
            foreach(var scenarioList in scenarioDict.Value)
            {
                newDict.Add(scenarioList.Progress, scenarioList);
            }
            ScenarioData.Add(scenarioDict.Key, newDict);
        }
    }
}
