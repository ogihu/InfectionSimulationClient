using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TissueBox : UsingItem
{
    GameObject _tissue;

    public override void Init()
    {
        _tissue = Util.FindChildByName(gameObject, "Tissue");
        base.Init();
    }

    public override bool Use(BaseController character)
    {
        Transform parent = Util.FindChildByName(character.gameObject, "L_hand_grap_point").transform;
        if (parent == null)
        {
            Debug.LogWarning("Can't find transform");
            return false;
        }
        gameObject.transform.SetParent(parent, false);

        Transform subParent = Util.FindChildByName(character.gameObject, "R_hand_grap_point").transform;
        if (subParent == null)
        {
            Debug.LogWarning("Can't find transform");
            return false;
        }
        _tissue.transform.SetParent(subParent, false);

        character.State = Google.Protobuf.Protocol.CreatureState.UsingTissue;

        return base.Use(character);
    }

    public override void UnUse()
    {
        Managers.Resource.Destroy(_tissue);
        base.UnUse();
    }
}
