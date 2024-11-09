using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public ItemInfo ItemInfo { get; set; }

    protected void Awake()
    {
        Init();
    }

    public virtual void Init() { } 

    public virtual bool Use(BaseController character)
    {
        gameObject.layer = character.gameObject.layer;
        return character.UseItem(this.gameObject);
    }

    public virtual void UnUse(BaseController character)
    {
        character.UnUseItem(this.gameObject.name);
    }
}
