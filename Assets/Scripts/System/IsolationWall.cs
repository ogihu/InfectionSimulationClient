using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsolationWall : MonoBehaviour
{
    Vector3 warpPosition = new Vector3(25.6f, 0, -34.6f);

    private void OnTriggerEnter(Collider other)
    {
        PlayerController pc = other.GetComponent<PlayerController>();
        
        if (pc == null)
            return;

        if(pc.EquipProtectsCount < 4)
        {
            if(pc is MyPlayerController mpc)
            {
                Managers.UI.CreateSystemPopup("WarningPopup", "보호구 없이 들어갈 수 없습니다.", UIManager.NoticeType.Warning);
            }

            pc.transform.position = warpPosition;
        }
    }
}
