

using System.Linq.Expressions;
using TMPro;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
    public bool PlayerUsingMic;
    public bool startcheck = false;
    UnityEngine.SceneManagement.Scene scene;
    string str = "Game";

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
        if (scene.Equals(str) && startcheck)
            return;

       scene = SceneManager.GetActiveScene();

        if (!UsingMic)
            ChangeMicStateFalse();
        else
            ChangeMicStateTrue();
    }

    public void ChangeMicStateFalse()
    {
        Managers.STT.MySpeech.SetActive(false);
        if (!Managers.Scenario._doingScenario)
            return;
        if(MicCheckUI == null)
             init();
        MicCheckUI.GetComponent<TMP_Text>().text = "키워드를 알맞은 칸에 넣으세요";
        if ((!MicCheckUI.activeSelf) && Managers.Scenario.CurrentScenarioInfo.Action == "Tell")
            MicCheckUI.SetActive(true);  
        else if (Managers.Scenario.CurrentScenarioInfo.Action != "Tell")
            MicCheckUI.SetActive(false);
    }

    public void ChangeMicStateTrue()
    {
        if (!Managers.Scenario._doingScenario)
        {
            Managers.STT.MySpeech.SetActive(false);
            return;
        }
            
        if (MicCheckUI == null)
            init();
        Managers.STT.MySpeech.SetActive(true);

        MicCheckUI.GetComponent<TMP_Text>().text = "키를 눌러 녹음을 시작하세요";
        if ((!MicCheckUI.activeSelf) && Managers.Scenario.CurrentScenarioInfo.Action == "Tell")
            MicCheckUI.SetActive(true);
        else if (Managers.Scenario.CurrentScenarioInfo.Action != "Tell")
        {
            MicCheckUI.SetActive(false);
            Managers.STT.MySpeech.SetActive(false);
        }
            
    }

    public void Init()
    {
        BGMVol = 1f;
        SFXVol = 1f;
        UsingMic = false;
        PlayerUsingMic = false;
    }

    public void UseCheckMic()
    {
        if(MicCheckUI == null)
        {
            MicCheckUI = GameObject.Find("CheckInferencing");
        }
        if(PlayerUsingMic)
        {
            MicCheckUI.GetComponent<TMP_Text>().text = "키를 눌러 녹음을 시작하세요.";
            PlayerUsingMic = false;
        }
            
        else
        {
            MicCheckUI.GetComponent<TMP_Text>().text = "키를 눌러 녹음을 중단하세요.";
            PlayerUsingMic = true;
        }
    }

    public void Clear()
    {
        MicCheckUI = null;
    }
}
