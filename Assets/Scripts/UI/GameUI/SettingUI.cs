using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Setting : MonoBehaviour
{
    private void OnEnable()
    {
        Managers.Object.MyPlayer.State = Google.Protobuf.Protocol.CreatureState.Setting;
    }

    private void OnDisable()
    {
        if(Managers.Object.MyPlayer != null)
            Managers.Object.MyPlayer.State = Google.Protobuf.Protocol.CreatureState.Idle;
    }
}
