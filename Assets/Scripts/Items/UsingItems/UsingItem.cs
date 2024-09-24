using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UsingItem : Item
{
    public override void UnUse(BaseController character)
    {
        character.State = Google.Protobuf.Protocol.CreatureState.Idle;
        base.UnUse(character);
    }
}