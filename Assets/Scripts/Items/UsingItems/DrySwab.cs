using Ricimi;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Google.Cloud.Speech.V1.LanguageCodes;

public class DrySwab : UsingItem
{
    Texture2D cursorTexture;  // 사용할 커서 이미지
    GameObject popup;
    GameObject lesion;

    GameObject patient;
    float raycastDistance = 3f;
    int _layerPoint;
    bool _choosingPatient = false;
    bool _used = false;
    GameObject noticeUI;

    Material[] baseMat;
    Material[] outlineMat;

    public override void Init()
    {
        base.Init();
        _layerPoint = 1 << LayerMask.NameToLayer("Patient");
        outlineMat = Resources.LoadAll<Material>("Materials/Characters/환자/Outline");
        cursorTexture = Resources.Load<Texture2D>("Sprites/itemIcons/Use_DrySwab");
    }

    public override bool Use(BaseController character)
    {
        if (Managers.Object.MyPlayer != character)
            return true;

        Managers.Item.SelectedItem.Using = true;
        _choosingPatient = true;
        noticeUI = Managers.UI.CreateUI("ItemTargetNotice");

        return base.Use(character);
    }

    private void Update()
    {
        if (_choosingPatient)
        {
            Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);

            RaycastHit hitInfo;

            if (Physics.Raycast(ray, out hitInfo, raycastDistance, _layerPoint))
            {
                patient = Util.FindParentByName(hitInfo.transform.gameObject, "환자");

                Renderer renderer = patient.GetComponent<Renderer>();

                if (baseMat == null)
                {
                    baseMat = renderer.materials.ToArray();
                }

                if (renderer != null)
                {
                    renderer.materials = outlineMat;
                }
            }
            else
            {
                if (patient != null)
                {
                    Renderer renderer = patient.GetComponent<Renderer>();

                    if (renderer != null)
                    {
                        renderer.materials = baseMat;
                    }

                    patient = null;
                }
            }

            if (patient != null && _used == false)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    _choosingPatient = false;
                    _used = true;
                    Managers.UI.DestroyUI(noticeUI);

                    Managers.Scenario.State_Image = true;
                    lesion = Managers.UI.CreateUI("LesionUI");
                    lesion.GetComponent<LesionUI>().Swab = this;

                    Vector2 hotSpot = new Vector2(0, cursorTexture.height);
                    Cursor.SetCursor(cursorTexture, hotSpot, CursorMode.ForceSoftware);

                    Renderer renderer = patient.GetComponent<Renderer>();

                    if (renderer != null)
                    {
                        renderer.materials = baseMat;
                    }
                }
            }
        }
    }

    public void CancelUse()
    {
        if(noticeUI != null)
            Managers.UI.DestroyUI(noticeUI);

        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        Managers.Scenario.State_Image = false;
        Managers.Item.ForceUnUseItem(ItemInfo);
        Managers.Scenario.MyAction = "";
        Managers.Scenario.Item = "";
        Managers.Resource.Destroy(this.gameObject);
    }

    public void CompleteLesion()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        Managers.Scenario.State_Image = false;
        Managers.Item.ForceDropItem(ItemInfo);
        Managers.Scenario.MyAction = "Use";
        Managers.Scenario.Item = "DrySwab";
        Managers.Scenario.Targets.Add("환자");
    }
}
