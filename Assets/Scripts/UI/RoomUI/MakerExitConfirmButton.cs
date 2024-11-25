using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MakerExitConfirmButton : ButtonUI
{
    protected override void OnClicked()
    {
        base.OnClicked();

        Managers.Scene.LoadScene(Define.Scene.Lobby);
    }
}
