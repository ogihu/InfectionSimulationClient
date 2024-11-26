using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SystemPopup : MonoBehaviour
{
    Coroutine _co;

    public void ChangeText(string notice)
    {
        gameObject.SetActive(true);

        if (_co != null)
            StopCoroutine(_co);

        if(GetComponent<TMP_Text>() != null)
        {
            GetComponent<TMP_Text>().text = notice;
            return;
        }

        GetComponentInChildren<TMP_Text>().text = notice;
    }

    public void AutoDestroy(float time)
    {
        if (_co != null)
            StopCoroutine(_co);

        _co = StartCoroutine(CoDestroyAfter(time));
    }

    IEnumerator CoDestroyAfter(float time)
    {
        yield return new WaitForSeconds(time);
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if(_co != null)
        {
            StopCoroutine(_co);
            _co = null;
        }
    }
}
