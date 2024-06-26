using Google.Protobuf.Protocol;
using System.Collections;
using UnityEngine;

public class MyPlayerController : PlayerController
{
	[SerializeField] private float _syncTimer = 0.5f;
	[SerializeField] float _camRotationSpeed;
	CameraArm _cameraArm;
	float mouseX = 0f;
	Coroutine _coSendPacket;

    protected override void Start()
    {
		base.Start();
		_cameraArm = GetComponentInChildren<CameraArm>();
		_coSendPacket = StartCoroutine(CoSyncUpdate());
    }

    protected override void UpdateController()
	{
		UpdateRotation();
		GetKeyInput();
		base.UpdateController();
	}

    protected override void UpdateMove()
    {
        base.UpdateMove();
		Pos = transform.position;
    }

    protected override void UpdateRotation()
    {
        _cameraArm.CameraRotation(_camRotationSpeed);
        mouseX += Input.GetAxis("Mouse X") * _camRotationSpeed;
		this.transform.localEulerAngles = new Vector3(0, mouseX, 0);
		Debug.Log($"카메라 방향 {Camera.main.transform.forward}");
		Dir = new Vector3(Camera.main.transform.forward.x, 0, Camera.main.transform.forward.z);
	}

	void GetKeyInput()
	{
		InputBit = Managers.Input.SetKeyInput(KeyCode.W, InputBit, () => { CheckUpdatedFlag(); });
		InputBit = Managers.Input.SetKeyInput(KeyCode.A, InputBit, () => { CheckUpdatedFlag(); });
		InputBit = Managers.Input.SetKeyInput(KeyCode.S, InputBit, () => { CheckUpdatedFlag(); });
		InputBit = Managers.Input.SetKeyInput(KeyCode.D, InputBit, () => { CheckUpdatedFlag(); });
	}

	private void SendSyncPacket()
    {
        if (_syncUpdated)
        {
			C_Sync syncPacket = new C_Sync();
			syncPacket.PosInfo = PosInfo;
			Managers.Network.Send(syncPacket);
			_syncUpdated = false;
		}
	}

	private void SendMovePacket()
	{
		if (_updated)
		{
			C_Move movePacket = new C_Move();
			movePacket.MoveInfo = MoveInfo;
			Managers.Network.Send(movePacket);
			_updated = false;
		}
	}

	private void CheckUpdatedFlag()
	{
		SendSyncPacket();
		SendMovePacket();
	}
	
	IEnumerator CoSyncUpdate()
    {
        while (true)
        {
			CheckUpdatedFlag();
			yield return new WaitForSeconds(_syncTimer);
        }
    }
}