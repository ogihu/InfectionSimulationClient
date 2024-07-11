using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCController : CreatureController
{
    protected override void UpdateMove()
    {
        if (transform.position == Pos)
        {
            State = CreatureState.Idle;
            return;
        }
    }
}
