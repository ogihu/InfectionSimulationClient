using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Define
{
    #region Voice

    public class VoiceBuffer<T>
    {
        private T[] _buffer;
        private int _writePos;
        private int _readPos;
        private int _bufferSize;
        private int _count;

        public VoiceBuffer(int size)
        {
            _bufferSize = size;
            _buffer = new T[_bufferSize];
            _writePos = 0;
            _readPos = 0;
            _count = 0;
        }

        public void Write(T item)
        {
            _buffer[_writePos] = item;
            _writePos = (_writePos + 1) % _bufferSize;

            if (_count == _bufferSize)
            {
                _readPos = (_readPos + 1) % _bufferSize;
            }
            else
            {
                _count++;
            }
        }

        public void Write(T[] items)
        {
            foreach (var item in items)
            {
                Write(item);
            }
        }

        public T Read()
        {
            if (_count == 0)
            {
                return default;
            }

            T item = _buffer[_readPos];
            _readPos = (_readPos + 1) % _bufferSize;
            _count--;

            return item;
        }

        public void Read(T[] output, int length)
        {
            if (_count < length)
                return;

            for (int i = 0; i < length; i++)
            {
                output[i] = Read();
            }
        }

        public int Count { get { return _count; } }
        public bool IsEmpty { get { return _count == 0; } }
        public bool IsFull { get { return _count == _bufferSize; } }
        public int ReadPos { get { return _readPos; } }
    }

    public static int VoiceFrequency = 16000;
    public static int VoiceChannel = 2;

    public static float[] ConvertMonoToStereo(float[] monoSamples)
    {
        float[] stereoSamples = new float[monoSamples.Length * 2];
        for (int i = 0; i < monoSamples.Length; i++)
        {
            stereoSamples[i * 2] = monoSamples[i];
            stereoSamples[i * 2 + 1] = monoSamples[i];
        }
        return stereoSamples;
    }

    public static float[] ConvertStereoToMono(float[] stereoSamples)
    {
        int monoLength = stereoSamples.Length / 2;
        float[] monoSamples = new float[monoLength];

        for (int i = 0; i < monoLength; i++)
        {
            monoSamples[i] = (stereoSamples[i * 2] + stereoSamples[i * 2 + 1]) / 2;
        }

        return monoSamples;
    }

    #endregion

    #region PointMapping

    //응급센터 메인 입구 위치
    public static readonly Vector3 Entrance = new Vector3(-6.5f, 0, 17.5f);
    //NPC들 시나리오 대기 위치
    public static readonly Vector3 WaitingArea = new Vector3(-35, 0, 15);
    public static int WaitingCount = 0;
    //음압격리실 위치
    public static readonly Vector3 IsolationArea = new Vector3(21, 0, -47.4f);
    public static readonly Vector3 BeforeIsolationArea = new Vector3(24.4f, 0, -34.7f);
    //환자 침대에 눕는 위치
    public static readonly Vector3 Patientlying = new Vector3(0.02f, 0.26f, 0.02f);
    //관찰구역 환자 침대 이송 시작하는 위치
    public static readonly Vector3 MovePosition = new Vector3(9.6f, 0, 2.32f);
    //미화 요원 소독 테이블 위치
    public static readonly Vector3 OATable = new Vector3(7, 0, 5);
    //관찰구역, 음압격리실 침대 위치
    public static readonly Vector3 OABed = new Vector3(9.5f, 0, 4);
    public static readonly Vector3 IABed = new Vector3(21, 0, -45);
    //보안요원들 통제 포인트
    public static readonly Vector3 BlockingPoint1 = new Vector3(-1, 0, 0);
    public static readonly Vector3 BlockingPoint2 = new Vector3(-1, 0, -11);
    public static readonly Vector3 BlockingPoint3 = new Vector3(-1, 0, -22);
    public static readonly Vector3 BlockingPoint4 = new Vector3(12, 0, -26.5f);
    //음압격리실 환자 침대 이송 시작하는 위치
    public static readonly Vector3 MovePosition2 = new Vector3(21.1f, 0, -43.7f);
    //음압격리실 외부 통로 위치
    public static readonly Vector3 IAInsideEntrancePoint = new Vector3(26.5f, 0, -27f);
    public static readonly Vector3 IAOutsideEntrancePoint = new Vector3(35.67f, 0, -27f);
    public static readonly Vector3 EmergencyAgentWaitingPoint = new Vector3(33.07f, 0, -29.13f);
    public static readonly Vector3 ChangeBedPoint = new Vector3(28.46f, 0, -27.07f);
    //PlayerNPC 스폰 위치
    public static readonly Vector3 PlayerNPCSpawnPoint = new Vector3(-12.6f, 0, 2);

    #endregion

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
        "진단검사의학팀",
        "아산충무병원",
        "의심환자"
    };

    #region VoiceMapping

    public static string ChangeText(string main)
    {
        // main을 공백으로 나눠 단어별로 split 배열에 저장합니다.
        string[] split = main.Split(' ');

        // 각 단어를 순회하며 STTMapping에서 해당 단어가 존재하는지 확인 후 교체합니다.
        for (int i = 0; i < split.Length; i++)
        {
            if (STTMapping.ContainsKey(split[i]))
            {
                split[i] = STTMapping[split[i]];
            }
        }

        // 교체된 단어들을 다시 공백으로 연결하여 문자열을 만듭니다.
        return string.Join(" ", split);
    }

    public static readonly Dictionary<string, string> STTMapping = new Dictionary<string, string>()
    {
        {"음악", "음압"},
        {"경리실", "격리실"},
        {"경비실", "격리실"},
        {"m폭스", "엠폭스"},
        {"mpox", "엠폭스"},
        {"20만", "의심환자"},
        {"신환자", "의심환자"},
        {"확진다", "확진자"},
        {"music", "유지"},
        {"여보", "여부"},
        {"석적", "접촉"},
        {"확진한다", "확진환자"},
        {"접속", "접촉"},
        {"가면", "감염"},
        {"전사", "전파"},
        {"잠바", "전파"},
        {"보고를 착용", "보호구를 착용"},
        {"박진", "확진"},
        {"유심론자", "의심환자"},
        {"사정", "4종"},
        {"보고서", "보호구"},
        {"소포", "수포"},
        {"관리 팀", "관리팀"},
        {"사전", "4종"},
        {"보고", "보호구"},
        {"위신", "의심"},
        {"혼자", "환자"},
        {"4동", "4종"},
        {"사종", "4종"},
        {"20년", "의심환자"},
        {"홍제", "통제"},
        {"1층", "의심"},
        {"20", "의심"}
    };

    #endregion

    #region Informations

    public class JScenarioInfo
    {
        public int Progress { get; set; }
        public string Place { get; set; }
        public string Position { get; set; }
        public string Item { get; set; }
        public string Action { get; set; }
        public string Sentence { get; set; }
        public string STTKeywords { get; set; }
        public string Targets { get; set; }
        public string DetailHint { get; set; }
        public string Hint { get; set; }
        public string Confirm { get; set; }
        public string Question { get; set; }
        public string Answers { get; set; }
        public string ObjectIndicator { get; set; }
    }

    public class ScenarioInfo
    {
        public int Progress { get; set; }
        public string Place { get; set; }
        public string Position { get; set; }
        public string Item { get; set; }
        public string Action { get; set; }
        public string Sentence { get; set; }
        public string OriginalSentence { get; set; }
        public List<string> GUIKeywords { get; set; } = new List<string>();
        public List<string> STTKeywords { get; set; } = new List<string>();
        public List<string> Targets { get; set; } = new List<string>();
        public string DetailHint { get; set; }
        public string Hint { get; set; }
        public string Confirm { get; set; }
        public string Question { get; set; }
        public List<string> Answers { get; set; } = new List<string>();
        public List<string> ObjectIndicator { get; set; } = new List<string>();
    }

    public class ItemInfo
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public ItemInfo(string name, string description)
        {
            this.Name = name;
            this.Description = description;
        }
    }

    public static readonly Dictionary<string, ItemInfo> ItemInfoDict = new Dictionary<string, ItemInfo>()
    {
        {"Mask", new ItemInfo("마스크", "먼지, 바이러스, 세균 등 공기 중의\n유해물질이나 감염으로부터 착용자를 보호")},
        {"ProtectedGear", new ItemInfo("방호복", "의료진을 감염성 물질로부터 보호하여 안전을 유지")},
        {"ShoeCover", new ItemInfo("덧신", "병원 내 감염 예방과 위생 유지, 환자 보호를 위해 사용")},
        {"Goggle", new ItemInfo("고글", "눈을 통한 감염을 방지하기 위해 착용하는 보호 장비")},
        {"Glove", new ItemInfo("장갑", "감염 예방과 위생 관리를 위해 의료진과\n환자 간의 직접적인 접촉을 차단하는 보호 장비")},
        {"Handwash", new ItemInfo("손소독제", "피부 살균, 소독의 목적으로 사용")},
        {"Syringe",new ItemInfo("주사기","약액(주사약)을 생물체의 체내에 주사하는 의료 기기") },
        {"DrySwab",new ItemInfo("의료용 면봉","의료 현장에서 사용하는 면봉으로\n병원체와 미생물 의 채취, 상처의 소독 등에 사용") }
    };

    public static readonly Dictionary<char, char> KtoE = new Dictionary<char, char>()
    {
        {'ㅂ', 'q'}, {'ㅈ', 'w'}, {'ㄷ', 'e'}, {'ㄱ', 'r'}, {'ㅅ', 't'},
        {'ㅛ', 'y'}, {'ㅕ', 'u'}, {'ㅑ', 'i'}, {'ㅐ', 'o'}, {'ㅔ', 'p'},
        {'ㅁ', 'a'}, {'ㄴ', 's'}, {'ㅇ', 'd'}, {'ㄹ', 'f'}, {'ㅎ', 'g'},
        {'ㅗ', 'h'}, {'ㅓ', 'j'}, {'ㅏ', 'k'}, {'ㅣ', 'l'},
        {'ㅋ', 'z'}, {'ㅌ', 'x'}, {'ㅊ', 'c'}, {'ㅍ', 'v'}, {'ㅠ', 'b'},
        {'ㅜ', 'n'}, {'ㅡ', 'm'}
    };

    public static char ChangeKtoE(char korean)
    {
        // 영어와 맵핑된 한글은 영어로 변경하여 반환
        if (KtoE.TryGetValue(korean, out char englishChar))
        {
            return englishChar;
        }

        // 맵핑되어 있지 않은 문자는 그대로 반환
        return korean;
    }

    #endregion

    #region Enums

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

    public enum Sound
    {
        Bgm,
        Effect,
        MaxCount
    }

    #endregion
}
