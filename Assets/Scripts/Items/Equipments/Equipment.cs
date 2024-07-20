using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Equipment : MonoBehaviour
{
    public virtual void Equip(BaseController character)
    {
        character.AddEquipment(this.gameObject);
    }
}