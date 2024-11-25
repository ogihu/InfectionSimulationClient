using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IAToOutCollider : MonoBehaviour
{
    Vector3 warpPosition = new Vector3(7.9f, 0, -24.6f);

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
