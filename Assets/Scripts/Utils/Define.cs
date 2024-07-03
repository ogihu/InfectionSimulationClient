using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Define
{
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
}
