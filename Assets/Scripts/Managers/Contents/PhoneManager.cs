using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhoneManager
{
    SmartPhone _device;
    public SmartPhone Device 
    {
        get
        {
            if(_device == null)
            {
                OpenPhone();
                ClosePhone();
            }

            return _device;
        }
    }

    public void OpenPhone()
    {
        Managers.Object.MyPlayer.State = CreatureState.UsingPhone;
        GameObject go = Managers.UI.CreateUI("SmartPhone");
        _device = go.GetComponent<SmartPhone>();
        return;
    }

    public void ClosePhone()
    {
        Managers.UI.DestroyUI(Device.gameObject);
        Managers.Object.MyPlayer.State = CreatureState.Idle;
    }

    public void Clear()
    {
        _device = null;
    }
}
