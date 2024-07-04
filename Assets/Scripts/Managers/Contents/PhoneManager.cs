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
            if (_device == null)
                _device = GameObject.Find("SmartPhone").GetOrAddComponent<SmartPhone>();

            return _device;
        }
    }

    public void OpenPhone()
    {
        _device = Managers.UI.CreateUI("SmartPhone").GetOrAddComponent<SmartPhone>();
    }

    public void ClosePhone()
    {
        Managers.UI.DestroyUI(Device.gameObject);
    }
}
