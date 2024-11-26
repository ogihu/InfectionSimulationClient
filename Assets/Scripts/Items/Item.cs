using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;

public class Item : MonoBehaviour
{
    public ItemInfo ItemInfo { get; set; }
    protected BaseController _owner;

    protected void Awake()
    {
        Init();
    }

    public virtual void Init() { } 

    public virtual bool Use(BaseController character)
    {
        if(gameObject.tag == "FaceEquipment" && character as MyPlayerController)
        {
            gameObject.layer = LayerMask.NameToLayer("FaceEquipment");
        }
        else
        {
            gameObject.layer = character.gameObject.layer;
        }

        _owner = character;

        if (_owner == Managers.Object.MyPlayer && !Managers.Object.MyPlayer.Items.ContainsKey(gameObject.name))
        {
            C_Equip equipPacket = new C_Equip();
            equipPacket.ItemName = gameObject.name;
            Managers.Network.Send(equipPacket);
        }

        return character.UseItem(this.gameObject);
    }

    public virtual void UnUse()
    {
        if (_owner == Managers.Object.MyPlayer && Managers.Object.MyPlayer.Items.ContainsKey(gameObject.name))
        {
            C_UnEquip unEquipPacket = new C_UnEquip();
            unEquipPacket.ItemName = gameObject.name;
            Managers.Network.Send(unEquipPacket);
        }

        _owner.UnUseItem(this.gameObject.name);
    }
}
