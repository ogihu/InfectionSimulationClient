using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhoneManager
{
    public SmartPhone Device { get; set; }

    public SmartPhone OpenPhone()
    {
        GameObject go = Managers.UI.CreateUI("SmartPhone");
        Device = go.GetComponent<SmartPhone>();
        return Device;
    }

    public void ClosePhone()
    {
        Managers.UI.DestroyUI(Device.gameObject);
    }
}
