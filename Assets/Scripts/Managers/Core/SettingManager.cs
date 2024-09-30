

using UnityEngine;
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

    public void Init()
    {
        BGMVol = 1f;
        SFXVol = 1f;
        UsingMic = false;
    }
}
