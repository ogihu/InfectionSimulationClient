using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EMRManager
{
    GameObject EMR;

    public bool DoingEMR => EMR != null;

    public void OpenEMRWrite()
    {
        EMR = Managers.UI.CreateUI("MpoxEMRWrite");
    }

    public void OpenEMRRead()
    {
        EMR = Managers.UI.CreateUI("MpoxEMRRead");
    }

    public void CloseEMR()
    {
        if (EMR == null)
            return;

        if(EMR.name == "MpoxEMRRead")
        {
            Managers.Scenario.MyAction = "EMRRead";
        }

        Managers.UI.DestroyUI(EMR);
        EMR = null;
    }
}
