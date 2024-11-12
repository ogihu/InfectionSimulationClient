using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        return character.UseItem(this.gameObject);
    }

    public virtual void UnUse()
    {
        if (_owner == Managers.Object.MyPlayer && Managers.Object.MyPlayer.Items.ContainsKey(ItemInfo.ItemData.Name))
        {
            C_UnEquip unEquipPacket = new C_UnEquip();
            unEquipPacket.ItemName = ItemInfo.ItemData.Name;
            Managers.Network.Send(unEquipPacket);
        }

        _owner.UnUseItem(this.gameObject.name);
    }
}
