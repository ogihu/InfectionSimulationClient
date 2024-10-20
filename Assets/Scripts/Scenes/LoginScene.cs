using UnityEngine;

public class LoginScene : BaseScene
{
    protected override void Init()
    {
        base.Init();

        SceneType = Define.Scene.Login;

        Application.runInBackground = true;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        GameObject go = GameObject.Find("MenuUI");
        if(go == null)
        {
            Managers.UI.CreateUI("MenuUI");
        }
    }

    public override void Clear()
    {

    }
}
