using Google.Protobuf.Protocol;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.EventSystems;
using System.Drawing;
using UnityEngine.TextCore.Text;
using TMPro;

public class Syringe : UsingItem
{
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
    }

    public override bool Use(BaseController character)
    {
        // 주사기 오브젝트를 손에 붙임
        gameObject.transform.SetParent(Util.FindChildByName(character.gameObject, "basic_rig L Hand").transform, false);
        gameObject.transform.localPosition = new Vector3(-0.0985f, 0.0297f, -0.0181f);
        gameObject.transform.localRotation = Quaternion.Euler(-0.215f, 133.05f, -82.805f);
        gameObject.transform.localScale = new Vector3(0.8333f, 0.8333f, 0.8333f);

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
                    noticeUI.GetComponent<TMP_Text>().text = "혈액 검체 채취 중...";
                    _choosingPatient = false;
                    _used = true;
                    StartCoroutine(CoDelayIdle(3));
                }
            }
        }
    }

    IEnumerator CoDelayIdle(float time)
    {
        Managers.Object.MyPlayer.State = CreatureState.Syringe;

        yield return new WaitForSeconds(time);

        Renderer renderer = patient.GetComponent<Renderer>();

        if (renderer != null)
        {
            renderer.materials = baseMat;
        }

        Managers.UI.DestroyUI(noticeUI);
        Managers.Scenario.Targets.Add("환자");

        Managers.Item.ForceDropItem(ItemInfo);
        Managers.Scenario.MyAction = "Use";

        if (Managers.Item.IsInventoryOpen == true)
        {
            Managers.Item.OpenOrCloseInventory();
        }

        Managers.Object.MyPlayer.State = CreatureState.Idle;
    }
}
