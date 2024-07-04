using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Setting : MonoBehaviour
{
    private void OnEnable()
    {
        Managers.Object.MyPlayer._playerState = Define.PlayerState.UsingSetting;
    }

    private void OnDisable()
    {
        Managers.Object.MyPlayer._playerState = Define.PlayerState.None;
    }
}
