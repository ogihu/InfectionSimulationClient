using Ricimi;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class ImageManager 
{
    GameObject ui;
    Image image;

    
    public IEnumerator ImageApear(string name, string situation = null, bool frist_Image = false)
    {
        Managers.Scenario.State_Image = true;
        GameObject popup = Managers.UI.CreateUI("PopupNotice"); ;

        if (frist_Image)
        {
            for (int i = 3; i > 0; i--)
            {
                if (popup == null)
                    yield break;
                popup.transform.GetChild(0).GetComponent<TMP_Text>().alignment = TextAlignmentOptions.Center;
                popup.transform.GetChild(0).GetComponent<TMP_Text>().text = i.ToString() + "초 뒤, " + situation + " 이미지가 제공됩니다.";
                yield return new WaitForSeconds(1f);
            }
        }

        Managers.UI.DestroyUI(popup);
        popup = null;
        ui = Managers.UI.CreateUI("Image_Apear");
        image = Util.FindChild(ui, "Image").GetComponent<Image>();
        image.sprite = Resources.Load<Sprite>($"Sprites/Disinfection/{name}");
        
        yield return Managers.Instance.StartCoroutine(CountApear(situation));
    }

    IEnumerator CountApear(string situation)
    {
        for (int i = 0; i< 10; i++)
        {
            if( image == null)
                break;
            yield return new WaitForSeconds(1f);
        }

        Managers.UI.DestroyUI(ui);
        Managers.Scenario.State_Image = false;
        ui = null;
        image = null;

        yield return null;
    }
}
