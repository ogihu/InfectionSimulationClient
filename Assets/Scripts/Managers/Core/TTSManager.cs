using UnityEngine;
using System.IO;
using System.Net;
using System;

public class TTSManager
{
    string apiURL = "https://texttospeech.googleapis.com/v1beta1/text:synthesize?key=AIzaSyACwjFh0MfjsFDjsCgCE6qsNU8UmUVrhYk";
    SetTextToSpeech tts = new SetTextToSpeech();

    public void Speaking(Transform host, string message)
    {
        AudioSource audioSource = host.GetComponent<AudioSource>();
        
        if (audioSource == null)
        {
            audioSource = host.gameObject.AddComponent<AudioSource>();
        }
        Init();
        tts.input.text = message;
        CreateAudio(audioSource);
    }

    void Init()
    {
        SetInput si = new SetInput();
        si.text = "";
        tts.input = si;

        SetVoice sv = new SetVoice();
        sv.languageCode = "ko-KR";
        sv.name = "ko-KR-Wavenet-C";
        sv.ssmlGender = "MALE";
        tts.voice = sv;

        SetAudioConfig sa = new SetAudioConfig();
        sa.audioEncoding = "LINEAR16";
        sa.speakingRate = 1f;
        sa.pitch = 0;
        sa.volumeGainDb = 0;
        tts.audioConfig = sa;
    }

    private void CreateAudio(AudioSource audioSource)
    {
        var str = TextToSpeechPost(tts);
        GetContent info = JsonUtility.FromJson<GetContent>(str);
        if (info == null)
            info = JsonUtility.FromJson<GetContent>(str);
        var bytes = Convert.FromBase64String(info.audioContent);

        var f = ConvertByteToFloat(bytes);

        AudioClip audioClip = AudioClip.Create("audioContent", f.Length, 1, 24000, false);
        audioClip.SetData(f, 0);

        audioSource.PlayOneShot(audioClip);
    }

    public static float[] ConvertByteToFloat(byte[] array)
    {
        float[] floatArr = new float[array.Length / 2];

        for (int i = 0; i < floatArr.Length; i++)
        {
            floatArr[i] = BitConverter.ToInt16(array, i * 2) / 32768.0f;
        }
        return floatArr;
    }

    public string TextToSpeechPost(object data)
    {
        //JsonUtility 사용. string을 요청 보내기 위한 byte[]로 변환
        string str = JsonUtility.ToJson(data);
        var bytes = System.Text.Encoding.UTF8.GetBytes(str);

        //요청을 보낼 주소와 세팅
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(apiURL);
        request.Method = "POST";
        request.ContentType = "application/json";
        request.ContentLength = bytes.Length;

        //Stream형식으로 데이터를 보냄 request
        try
        {
            using (var stream = request.GetRequestStream())
            {
                stream.Write(bytes, 0, bytes.Length);
                stream.Flush();
                stream.Close();
            }

            //요청 데이터에 대한 응답 데이터를 StreamReader로 받음
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            //StreamReader로 stream 데이터를 읽음
            StreamReader reader = new StreamReader(response.GetResponseStream());
            //stream 데이터를 string으로 변환
            string json = reader.ReadToEnd();
            return json;
        }
        catch (WebException e)
        {
            //log
            Debug.Log(e);
            return null;
        }
    }
}
public class SetTextToSpeech
{
    //input 필드에서 text와 ssml을 사용할 수 있고 둘 중 하나만 사용가능 반드시 하나는 사용해야함
    public SetInput input;
    //목소리에 관한부분
    public SetVoice voice;
    //오디오 형식에 관한 부분
    public SetAudioConfig audioConfig;
}
[System.Serializable]
public class SetInput
{
    public string text;

    //public string ssml; SSML은 음성 합성 마크업 언어(사용하지 않으면 주석)
}
[System.Serializable]
public class SetVoice
{
    //언어 설정, 영어 한국어 등
    public string languageCode;
    //목소리 이름(버전)
    public string name;
    //성별
    public string ssmlGender;
}
[System.Serializable]
public class SetAudioConfig
{
    //오디오 인코딩 방식
    public string audioEncoding;
    //말하는 속도
    public float speakingRate;
    //높낮이
    public int pitch;
    //크기
    public int volumeGainDb;
}
[System.Serializable]
public class GetContent
{
    public string audioContent;
}

