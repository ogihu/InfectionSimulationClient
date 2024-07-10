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
            {
                GameObject device = GameObject.Find("SmartPhone");

                if (device == null)
                {
                    _device = OpenPhone();
                    ClosePhone();
                }
                else
                    _device = device.GetOrAddComponent<SmartPhone>();
            }

            return _device;
        }
    }

    public SmartPhone OpenPhone()
    {
        SmartPhone device = Managers.UI.CreateUI("SmartPhone").GetOrAddComponent<SmartPhone>();
        return device;
    }

    public void ClosePhone()
    {
        Managers.UI.DestroyUI(Device.gameObject);
    }
}
