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
                GameObject device = Util.FindChildByName(Managers.UI.overlayCanvas.gameObject, "SmartPhone");

                if (device == null)
                {
                    _device = Managers.UI.CreateUI("SmartPhone").GetOrAddComponent<SmartPhone>();
                }
                else
                    _device = device.GetOrAddComponent<SmartPhone>();
            }

            return _device;
        }
    }

    public SmartPhone OpenPhone()
    {
        Device.gameObject.SetActive(true);
        return Device;
    }

    public void ClosePhone()
    {
        Managers.UI.DestroyUI(Device.gameObject);
    }
}
