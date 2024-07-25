using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProtectiveGear : Equipment
{
    public override void Equip(BaseController character)
    {
        base.Equip(character);

        character.LoadMeshAndMat("ProtectedGear");
    }

    public override void UnEquip(BaseController character)
    {
        character.LoadMeshAndMat(character.Position);

        base.UnEquip(character);
    }
}
