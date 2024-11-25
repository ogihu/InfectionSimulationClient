using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeywordManager
{
    GUIKeywordPanel _panel;
    public bool CanClose = true;
    public bool CanOpen = true;

    public void OpenGUIKeyword()
    {
        if (Managers.Scenario.CurrentScenarioInfo == null)
        {
            Managers.UI.CreateSystemPopup("WarningPopup", "시나리오가 시작되지 않았습니다.", UIManager.NoticeType.Warning);
            return;
        }

        if (Managers.Scenario.CurrentScenarioInfo.Position != Managers.Object.MyPlayer.Position)
        {
            Managers.UI.CreateSystemPopup("WarningPopup", "시나리오를 수행할 차례가 아닙니다.", UIManager.NoticeType.None);
            return;
        }

        if(CanOpen == false)
        {
            Managers.UI.CreateSystemPopup("WarningPopup", "이미 수행하셨습니다. 잠시만 기다려주세요.", UIManager.NoticeType.None);
            return;
        }

        GameObject go = Managers.UI.CreateUI("GUIKeyword/GUIKeywordPanel");
        go.GetComponent<GUIKeywordPanel>().UpdateUI();
        _panel = go.GetComponent<GUIKeywordPanel>();
        Managers.Object.MyPlayer.State = CreatureState.Conversation;
    }


    public void CloseGUIKeyword()
    {
        _panel.ResetUIs();
        Managers.UI.DestroyUI(_panel.gameObject);
        _panel = null;
        Managers.Object.MyPlayer.State = CreatureState.Idle;
    }


    public void CheckRemainKeywords()
    {
        if (_panel == null)
            return;

        if (!_panel.CheckRemainKeywords())
            return;

        C_Talk talkPacket = new C_Talk();
        talkPacket.Message = Managers.Scenario.CurrentScenarioInfo.OriginalSentence;
        talkPacket.TTSSelf = false;
        Managers.Network.Send(talkPacket);

        Managers.TTS.Speaking(Managers.Object.MyPlayer.transform, Managers.Scenario.CurrentScenarioInfo.OriginalSentence);
        Managers.STT.UpdateMySpeech(talkPacket.Message);

        GameObject effectUI = Managers.UI.CreateUI("EffectUI");
        CanClose = false;
        CanOpen = false;
        Managers.Instance.StartCoroutine(CoCloseGUIKeywordAfterDelay(3f, effectUI));
    }

    private IEnumerator CoCloseGUIKeywordAfterDelay(float delay, GameObject go)
    {
        yield return new WaitForSeconds(delay); 
        CloseGUIKeyword();
        Managers.Scenario.PassSpeech = true;
        CanClose = true;
        Managers.UI.DestroyUI(go);
    }

    public void Clear()
    {
        _panel = null;
    }
}