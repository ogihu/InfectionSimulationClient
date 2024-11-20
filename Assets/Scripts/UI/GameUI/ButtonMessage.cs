using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ButtonMessage : ButtonUI
{
    Message _info;
    public static GameObject Message;

    public void Init(Message info)
    {
        _info = info;
        transform.GetChild(0).GetComponent<TMP_Text>().text = $"     {info.Sender}";
    }

    protected override void OnClicked()
    {
        base.OnClicked();
        
        if(Message == null)
        {
            Message = Managers.UI.CreateUI("MessagePopup");
        }
        Message.transform.GetChild(0).GetComponent<TMP_Text>().text = $"From. {_info.Sender}\n\n{_info.Content}";
    }

    private void OnDisable()
    {
        Managers.UI.DestroyUI(this.gameObject);
        Debug.Log("메시지 비활성화");
    }
}
