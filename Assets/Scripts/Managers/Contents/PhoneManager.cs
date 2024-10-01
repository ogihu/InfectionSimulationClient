using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhoneManager
{
    public SmartPhone Device { get; private set; }

    public void OpenPhone()
    {
        if(Device != null)
        {
            ClosePhone();
            return;
        }

        Managers.Object.MyPlayer.State = CreatureState.UsingPhone;
        GameObject go = Managers.UI.CreateUI("SmartPhone");
        Device = go.GetComponent<SmartPhone>();
        return;
    }

    public void ClosePhone()
    {
        if(Device == null)
        {
            OpenPhone();
            return;
        }

        Managers.UI.DestroyUI(Device.gameObject);
        Device = null;
        Managers.Object.MyPlayer.State = CreatureState.Idle;
    }
}
