using UnityEngine;

public class LobbyScene : BaseScene
{
    protected override void Init()
    {
        base.Init();

        SceneType = Define.Scene.Lobby;

        Application.runInBackground = true;
    }

    public override void Clear()
    {

    }
}
