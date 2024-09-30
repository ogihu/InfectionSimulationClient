using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhoneManager
{
    public SmartPhone _device;
    public SmartPhone Device
    {
        get 
        {
            if (_device == null)
            {
                OpenPhone();
                ClosePhone();
            }

            return _device;
        }
    }

    public SmartPhone OpenPhone()
    {
        GameObject go = Managers.UI.CreateUI("SmartPhone");
        _device = go.GetComponent<SmartPhone>();
        return Device;
    }

    public void ClosePhone()
    {
        Managers.UI.DestroyUI(Device.gameObject);
    }
}
