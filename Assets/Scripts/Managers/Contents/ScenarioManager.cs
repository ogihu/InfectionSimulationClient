using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Whisper;
using static Define;

public class ScenarioManager
{
    int CompleteCount { get; set; }

    public string ScenarioName { get; set; }
    public int Progress { get; set; }
    public GameObject Patient { get; set; }
    public GameObject SpeechRecognitor { get; set; }
    public string SpeechText { get; set; }  //STT 결과 저장

    /// <summary>
    /// 이미 Complete 패킷을 보냈는지 확인, 서버에서 시나리오는 진행시키면 (NextProcess 패킷을 받으면) false로 전환해야 함.
    /// </summary>
    public bool _checkComplete;

    ScenarioInfo CurrentScenarioInfo { get; set; }

    public void Init(string scenarioName)
    {
        ScenarioName = scenarioName;
        Progress = 0;
        CompleteCount = 0;
        _checkComplete = false;
        SpeechText = null;
        CurrentScenarioInfo = Managers.Data.ScenarioData[ScenarioName][Progress];
        Patient = Managers.Resource.Instantiate("Creatures/NPC/MalePatient");
    }

    public void NextProgress(int progress)
    {
        Progress = progress;
        CompleteCount = 0;
        _checkComplete = false;
        SpeechText = null;
        CurrentScenarioInfo = Managers.Data.ScenarioData[ScenarioName][Progress];
        Managers.UI.ClearChat();
    }

    public void SendScenarioInfo(string scenarioName)
    {
        C_StartScenario scenarioPacket = new C_StartScenario();
        scenarioPacket.ScenarioName = scenarioName;
        Managers.Network.Send(scenarioPacket);
    }

    public void StartScenario(string scenarioName)
    {
        Managers.Instance.StartCoroutine(CoScenario(scenarioName));
    }

    IEnumerator CoScenario(string scenarioName)
    {
        Managers.UI.CreatePopup($"{scenarioName} 시나리오를 시작합니다.");
        yield return new WaitForSeconds(3.0f);
        Init(scenarioName);

        switch (scenarioName)
        {
            case "엠폭스":
                Patient.transform.position = new Vector3(11.5f, 0, 0);
                Patient.transform.rotation = Quaternion.Euler(0, -90, 0);
                Managers.UI.CreatePopup($"사랑합니다. 지금부터 신종감염병 대응 모의 훈련을 시작하고자 하오니 환자 및 보호자께서는 동요하지 마시기 바랍니다. 모의 훈련 요원들은 지금부터 훈련을 시작하도록 하겠습니다.");
                yield return new WaitForSeconds(3.0f);

                Managers.UI.CreateChatUI(Patient.transform, "선생님 방금 가족 중에 한명이 보건소로부터 엠폭스 확진받았다고 연락을 받아서요. 저도 곧 보건소로부터 연락올거라고 합니다.");
                if (Managers.Object.MyPlayer.Position == "응급센터 간호사1")
                {
                    Managers.Instance.StartCoroutine(CoCheckSpeech());
                    yield return new WaitUntil(() => CompleteCount >= 1);
                }
                SendComplete();

                yield return new WaitUntil(() => Progress == 1);

                Managers.UI.CreateChatUI(Patient.transform, "이관리 980421 입니다. 같이 살고있어요.");
                if (Managers.Object.MyPlayer.Position == "응급센터 간호사1")
                {
                    Managers.Instance.StartCoroutine(CoCheckSpeech());
                    yield return new WaitUntil(() => CompleteCount >= 1);
                }
                SendComplete();

                yield return new WaitUntil(() => Progress == 2);

                Managers.UI.ClearChat();
                if (Managers.Object.MyPlayer.Position == "응급의학과 의사")
                {
                    Managers.Instance.StartCoroutine(CoCheckSpeech());
                    yield return new WaitUntil(() => CompleteCount >= 1);
                }
                SendComplete();

                yield return new WaitUntil(() => Progress == 3);

                Managers.UI.CreatePopup("'#1 엠폭스 의심환자 발생 - 관찰구역에서 의심환자 대기' 시나리오를 완료했습니다.");
                break;
        }
    }

    void SendComplete()
    {
        if(_checkComplete == false)
        {
            C_Complete packet = new C_Complete();
            Managers.Network.Send(packet);
            _checkComplete = true;
        }
    }

    #region Speech Check
    
    IEnumerator CoCheckSpeech()
    {
        ChangeKeyword(CurrentScenarioInfo.Keywords);
        bool complete = false;

        while (!complete)
        {
            complete = CheckSpeech();
            yield return new WaitForSeconds(1.0f);
        }

        CompleteCount++;
    }

    void ChangeKeyword(List<string> keywords)
    {
        if (SpeechRecognitor == null)
            SpeechRecognitor = GameObject.Find("SpeechRecognitor");

        SpeechRecognitor.GetComponent<WhisperManager>().initialPrompt = "";
        foreach(var keyword in keywords)
        {
            SpeechRecognitor.GetComponent<WhisperManager>().initialPrompt += $"{keyword} ";
        }
    }

    bool CheckSpeech()
    {
        if (SpeechText == null)
            return false;

        int count = 0;

        foreach(var keyword in CurrentScenarioInfo.Keywords)
        {
            if (SpeechText.Contains(keyword))
                count++;
        }

        float ratio = (float)count / (float)CurrentScenarioInfo.Keywords.Count;

        GameObject go = Managers.UI.CreateUI("MySpeech");
        go.GetComponentInChildren<TMP_Text>().text = $"{SpeechText}\n정확도 {ratio * 100}%";
        Managers.Instance.StartCoroutine(Managers.UI.DestroyAfter(go, 3.0f));
        SpeechText = null;

        if (ratio > 0.7f)
            return true;
        else
            return false;
    }

    #endregion
}
