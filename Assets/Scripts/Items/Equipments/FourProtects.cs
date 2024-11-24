using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FourProtects : Item
{
    Item _glove;
    Item _goggle;
    Item _mask;
    Item _protectedGear;

    public override bool Use(BaseController character)
    {
        if (!base.Use(character))
            return false;

        _glove = Managers.Resource.Instantiate("Items/Glove").GetComponent<Item>();
        _goggle = Managers.Resource.Instantiate("Items/Goggle").GetComponent<Item>();
        _mask = Managers.Resource.Instantiate("Items/Mask").GetComponent<Item>();
        _protectedGear = Managers.Resource.Instantiate("Items/ProectedGear").GetComponent<Item>();

        _glove.Use(character);
        _goggle.Use(character);
        _mask.Use(character);
        _protectedGear.Use(character);

        return true;
    }

    public override void UnUse()
    {
        _glove.UnUse();
        _goggle.UnUse();
        _mask.UnUse();
        _protectedGear.UnUse();

        base.UnUse();
    }
}
