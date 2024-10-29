using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EMRWrite : MonoBehaviour
{
    int _rightCount = 0;

    public void IncreaseCount()
    {
        _rightCount++;

        if(_rightCount >= 6)
        {
            Managers.Scenario.MyAction = "EMRWrite";
            Managers.UI.DestroyUI(this.gameObject);
        }
    }
}