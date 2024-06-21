using UnityEngine;

public class GameScene : BaseScene
{
    protected override void Init()
    {
        base.Init();

        SceneType = Define.Scene.Game;

        Application.runInBackground = true;
    }

    public override void Clear()
    {
        
    }
}
