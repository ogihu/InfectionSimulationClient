
using UnityEngine;

public class GameScene : BaseScene
{
    //public static GameObject YudoLine;

    protected override void Init()
    {
        base.Init();

        SceneType = Define.Scene.Game;

        Application.runInBackground = true;
        Managers.STT.MySpeech.SetActive(false);
        Managers.Map.Init();
        //YudoLine = GameObject.Find("YudoLine");
        //YudoLine.SetActive(false);
    }

    public override void Clear()
    {

    }
}
