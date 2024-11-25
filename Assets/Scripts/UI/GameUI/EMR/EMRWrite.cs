using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EMRWrite : MonoBehaviour
{
    int _rightCount = 0;

    public void IncreaseCount()
    {
        _rightCount++;

        if (_rightCount >= 6)
        {
            GameObject effectUI = Managers.UI.CreateUI("EffectUI");
            Managers.EMR.CanClose = false;
            Managers.Instance.StartCoroutine(CoCloseEMRAfterDelay(3f, effectUI));
        }
    }

    private IEnumerator CoCloseEMRAfterDelay(float delay, GameObject effectUI)
    {
        yield return new WaitForSeconds(delay);

        Managers.EMR.CanClose = true;
        Managers.EMR.CloseEMR();
        Managers.Scenario.MyAction = "EMRWrite";

        if (effectUI != null)
        {
            Managers.UI.DestroyUI(effectUI);
        }
    }
}