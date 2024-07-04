using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Define
{
    public static readonly string[] PhoneAddress =
    {
        "응급센터",
        "응급의학과",
        "감염관리팀",
        "영상의학팀",
        "감염병대응센터",
        "감염재난대책본부",
        "안전이송실",
        "보안팀",
        "미화팀",
        "보건소",
        "원무팀",
        "진단검사의학팀"
    };

    public enum Sound
    {
        Bgm,
        Effect,
        MaxCount
    }

    public class ScenarioInfo
    {
        public int Progress { get; set; }
        public string Place { get; set; }
        public string Position { get; set; }
        public List<string> Keywords { get; set; } = new List<string>();
    }

    public enum Scene
    {
        Unknown,
        Game,
        Lobby,
        Login
    }

    public enum PlayerState
    {
        None,
        UsingPhone,
        UsingSetting
    }
}
