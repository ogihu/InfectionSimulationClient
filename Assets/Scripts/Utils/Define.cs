using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Define
{
    public static readonly Vector3 Entrance = new Vector3(-4, 0, 18);
    public static readonly Vector3 WaitingArea = new Vector3(-35, 0, 15);
    public static readonly Vector3 ObservationArea = new Vector3(11.5f, 0, 0);
    public static readonly Vector3 IsolationArea = new Vector3(22, 0, -44);

    public static readonly string[] PhoneAddress =
    {
        "응급센터",
        "응급의학과",
        "감염관리팀",
        "영상의학팀",
        "감염병대응센터",
        "감염재난대책본부",
        "이송팀",
        "보안팀",
        "미화팀",
        "원무팀",
        "진단검사의학팀"
    };

    public enum Sound
    {
        Bgm,
        Effect,
        MaxCount
    }

    public class JScenarioInfo
    {
        public int Progress { get; set; }
        public string Place { get; set; }
        public string Position { get; set; }
        public string Equipment { get; set; }
        public string Action { get; set; }
        public string Keywords { get; set; }
        public string Targets { get; set; }
        public string Speech { get; set; }
    }

    public class ScenarioInfo
    {
        public int Progress { get; set; }
        public string Place { get; set; }
        public string Position { get; set; }
        public string Equipment { get; set; }
        public string Action { get; set; }
        public List<string> Keywords { get; set; } = new List<string>();
        public List<string> Targets { get; set; } = new List<string>();
        public string Speech { get; set; }
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
