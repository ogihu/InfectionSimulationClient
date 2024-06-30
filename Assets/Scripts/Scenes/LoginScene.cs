using UnityEngine;

public class LoginScene : BaseScene
{
    protected override void Init()
    {
        base.Init();

        SceneType = Define.Scene.Login;

        Application.runInBackground = true;

        GameObject go = GameObject.Find("LoginUI");
        if(go == null)
        {
            Managers.UI.CreateUI("LoginUI");
        }
    }

    public override void Clear()
    {

    }
}
