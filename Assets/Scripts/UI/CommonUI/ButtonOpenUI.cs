using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonOpenUI : ButtonUI
{
    [Tooltip("생성하려는 UI 이름 입력")]
    public string uiName;

    protected override void OnClicked()
    {
        base.OnClicked();

        if(uiName == null)
        {
            Debug.LogError("Check the UI name field, it can't be null");
            return;
        }

        if(Managers.Scene.CurrentScene is LoginScene || Managers.Scene.CurrentScene is LobbyScene || Managers.Scene.CurrentScene is RoomScene)
        {
            Managers.UI.CreateUI(uiName);
            return;
        }

        if(Managers.Scene.CurrentScene is GameScene)
            if(Managers.Object.MyPlayer.IsCanActive())
                Managers.UI.CreateUI(uiName);
    }
}
