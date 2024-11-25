using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OAToIACollider : MonoBehaviour
{
    Vector3 warpPosition = new Vector3(-7.3f, 0, -9);

    private void OnTriggerStay(Collider other)
    {
        PlayerController pc = other.GetComponent<PlayerController>();

        if (pc == null)
            return;

        if (pc is MyPlayerController mpc)
        {
            Managers.UI.CreateSystemPopup("WarningPopup", "접근 금지", UIManager.NoticeType.Warning);
        }

        pc.transform.position = warpPosition;
    }
}
