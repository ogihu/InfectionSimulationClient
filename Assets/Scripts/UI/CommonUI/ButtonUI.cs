using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonUI : MonoBehaviour
{
    protected virtual void Awake()
    {
        GetComponent<Button>().onClick.AddListener(OnClicked);
    }

    protected virtual void OnClicked()
    {
        //버튼 눌렀을 때 필요한 이벤트(ex. 사운드)
    }
}
