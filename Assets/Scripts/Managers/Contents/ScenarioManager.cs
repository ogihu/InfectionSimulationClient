using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScenarioManager : MonoBehaviour
{
    private static ScenarioManager _instance = new ScenarioManager();
    public static ScenarioManager Instance { get { return _instance; } }

    public int Progress { get; set; }

    public void Init()
    {
        Progress = 0;
    }

    public void StartScenario(string scenarioName)
    {
        Init();
        StartCoroutine(CoScenario(scenarioName));
    }

    IEnumerator CoScenario(string scenarioName)
    {
        switch (scenarioName)
        {
            case "¿¥Æø½º":
                yield return new WaitUntil(() => Progress == 1);
                break;
        }
    }
}
