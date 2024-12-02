using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeforeIsolationWall : MonoBehaviour
{
    Vector3 warpPosition = new Vector3(25.6f, 0, -34.6f);

    private void OnTriggerStay(Collider other)
    {
        PlayerController pc = other.GetComponent<PlayerController>();

        if (pc == null)
            return;

        if (pc.EquipProtectsCount > 0)
        {
            if (pc is MyPlayerController mpc)
            {
                Managers.UI.CreateSystemPopup("WarningPopup", "보호구를 착의하고 나갈 수 없습니다.", UIManager.NoticeType.Warning);
            }

            pc.transform.position = warpPosition;
        }
    }
}
