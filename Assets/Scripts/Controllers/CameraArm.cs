using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraArm : MonoBehaviour
{
    [SerializeField] private float minCameraVertical;
    [SerializeField] private float maxCameraVertical;

    private float mouseY = 0f;

    public void CameraRotation(float rotationSpeed)
    {
        mouseY += Input.GetAxis("Mouse Y") * rotationSpeed;

        mouseY = Mathf.Clamp(mouseY, minCameraVertical, maxCameraVertical);
        this.transform.localEulerAngles = new Vector3(-mouseY, 0, 0);
    }
}
