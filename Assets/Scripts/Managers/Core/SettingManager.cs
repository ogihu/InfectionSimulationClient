using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SettingManager
{
    #region Sound

    float bgmVol;
    public float BGMVol
    {
        get
        {
            return bgmVol;
        }
        set
        {
            bgmVol = value;
            Managers.Sound.SetVolume(Define.Sound.Bgm);
        }
    }

    float sfxVol;
    public float SFXVol
    {
        get
        {
            return sfxVol;
        }
        set
        {
            sfxVol = value;
            Managers.Sound.SetVolume(Define.Sound.Effect);
        }
    }

    #endregion

    public bool UsingMic { get; set; }
    public string SelectedMic { get; set; }

    public GameObject MicCheckUI;

    void init()
    {
        MicCheckUI = GameObject.Find("CheckInferencing");
        if (MicCheckUI == null)
            return;
    }

    public void SceneStartMicCheck()
    {
        if(MicCheckUI == null)
            init();

        if(Managers.Scenario.CurrentScenarioInfo == null)
        {
            MicCheckUI.SetActive(true);
            MicCheckUI.GetComponent<TMP_Text>().text = "를 눌러 시나리오를 시작하세요.";
            return;
        }

        if (Managers.Scenario.CurrentScenarioInfo.Action == "MPX_Clothing" && Managers.Scenario.CurrentScenarioInfo.Position==Managers.Object.MyPlayer.Position)
        {
            MicCheckUI.SetActive(true);
            MicCheckUI.GetComponent<TMP_Text>().text = "를 눌러 보호구를 착용하세요";
            return;
        }
        else if (Managers.Scenario.CurrentScenarioInfo.Action == "MPX_LayOff" && Managers.Scenario.CurrentScenarioInfo.Position==Managers.Object.MyPlayer.Position)
        {
            MicCheckUI.SetActive(true);
            MicCheckUI.GetComponent<TMP_Text>().text = "를 눌러 보호구를 해제하세요";
            return;
        }

        if (Managers.Scenario.CurrentScenarioInfo != null && Managers.Scenario.CurrentScenarioInfo.Action == "Tell")
            MicCheckUI.SetActive(true);
        else
            MicCheckUI.SetActive(false);

        if (!UsingMic)
            ChangeMicStateFalse();
        else
            ChangeMicStateTrue();
    }

    public void ChangeMicStateFalse()
    {
        if (!Managers.Scenario._doingScenario)
        {
            Managers.STT.MySpeech.SetActive(false);
            return;
        }

        if (Managers.Scenario.CurrentScenarioInfo == null)
            return;

        MicCheckUI.SetActive(false);
        Managers.STT.MySpeech.SetActive(true);

        MicCheckUI.GetComponent<TMP_Text>().text = "키워드를 알맞은 칸에 넣으세요";

        if ((!MicCheckUI.activeSelf) && Managers.Scenario.CurrentScenarioInfo.Action == "Tell" && Managers.Scenario.CurrentScenarioInfo.Position==Managers.Object.MyPlayer.Position)
            MicCheckUI.SetActive(true);  

        else if (Managers.Scenario.CurrentScenarioInfo.Action != "Tell" && Managers.Scenario.CurrentScenarioInfo.Position != Managers.Object.MyPlayer.Position)
            MicCheckUI.SetActive(false);
    }

    public void ChangeMicStateTrue()
    {
        if (!Managers.Scenario._doingScenario)
        {
            Managers.STT.MySpeech.SetActive(false);
            return;
        }

        if (Managers.Scenario.CurrentScenarioInfo == null)
            return;

        if (MicCheckUI == null)
            init();

        Managers.STT.MySpeech.SetActive(true);

        Managers.STT.SetMic();
        MicCheckUI.GetComponent<TMP_Text>().text = "키를 눌러 녹음을 시작하세요";   
    }

    public void Init()
    {
        BGMVol = 1f;
        SFXVol = 1f;
        UsingMic = false;
    }

    public void UseCheckMic(bool usingMic)
    {
        if(MicCheckUI == null)
        {
            MicCheckUI = GameObject.Find("CheckInferencing");
        }

        if(!usingMic)
        {
            MicCheckUI.GetComponent<TMP_Text>().text = "키를 눌러 녹음을 시작하세요.";
        }
        else
        {
            MicCheckUI.GetComponent<TMP_Text>().text = "키를 눌러 녹음을 중단하세요.";
        }
    }

    public void Clear()
    {
        MicCheckUI = null;
    }
}
