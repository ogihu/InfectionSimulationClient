using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mask : Equipment
{
    public override void Equip(BaseController character)
    {
        base.Equip(character);
        
        Transform parent = Util.FindChildByName(character.gameObject, "basic_rig Head").transform;
        if(parent == null)
        {
            Debug.Log("Can't find transform");
            return;
        }

        gameObject.transform.SetParent(parent.transform, false);
        //gameObject.transform.localPosition = new Vector3(0.214f, -0.002f, -0.001f);
        //gameObject.transform.localRotation = Quaternion.Euler(new Vector3(-89.96f, 0, 90.046f));
        //gameObject.transform.localScale = new Vector3(1.091504f, 0.8167786f, 1.132167f);
    }
}
