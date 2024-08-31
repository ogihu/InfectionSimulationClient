using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TwinEquipment : Equipment
{
    protected GameObject _leftEquipment;
    protected GameObject _rightEquipment;

    public override void Init()
    {
        if (gameObject.name.Contains("(Clone)"))
        {
            gameObject.name = gameObject.name.Replace("(Clone)", "");
        }

        _leftEquipment = Util.FindChildByName(gameObject, $"{gameObject.name}L");
        _rightEquipment = Util.FindChildByName(gameObject, $"{gameObject.name}R");
    }

    public override void UnEquip(BaseController character)
    {
        Managers.Resource.Destroy(_leftEquipment);
        Managers.Resource.Destroy(_rightEquipment);

        base.UnEquip(character);
    }
}
