using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Setting : MonoBehaviour
{
    private void OnEnable()
    {
        if (Managers.Object.MyPlayer != null)
            Managers.Object.MyPlayer.State = Google.Protobuf.Protocol.CreatureState.Setting;

        if(Managers.Scene.CurrentScene as GameScene)
        {
            GameObject lobbyButton = Util.FindChildByName(gameObject, "BackToLobbyButton");
            lobbyButton.SetActive(true);
        }
        else
        {
            GameObject lobbyButton = Util.FindChildByName(gameObject, "BackToLobbyButton");
            lobbyButton.SetActive(false);
        }
    }

    private void OnDisable()
    {
        if(Managers.Object.MyPlayer != null)
            Managers.Object.MyPlayer.State = Google.Protobuf.Protocol.CreatureState.Idle;
    }
}
