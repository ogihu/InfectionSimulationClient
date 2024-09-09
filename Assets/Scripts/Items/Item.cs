using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    protected void Awake()
    {
        Init();
    }

    public virtual void Init() { } 

    public virtual bool Use(BaseController character)
    {
        return character.UseItem(this.gameObject);
    }

    public virtual void UnUse(BaseController character)
    {

    }
}
