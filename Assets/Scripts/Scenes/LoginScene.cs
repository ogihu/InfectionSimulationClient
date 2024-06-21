using UnityEngine;

public class LoginScene : BaseScene
{
    protected override void Init()
    {
        base.Init();

        SceneType = Define.Scene.Login;

        Application.runInBackground = true;
    }

    public override void Clear()
    {

    }
}
