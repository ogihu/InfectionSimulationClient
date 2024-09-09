using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Equipment : Item
{
    public override bool Use(BaseController character)
    {
        gameObject.layer = character.gameObject.layer;
        
        return base.Use(character);
    }

    public override void UnUse(BaseController character)
    {
        character.UnUseItem(this.gameObject.name);
    }
}