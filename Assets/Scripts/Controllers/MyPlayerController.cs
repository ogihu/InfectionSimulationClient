using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyPlayerController : PlayerController
{
	[SerializeField] private float _syncTimer = 0.5f;
	[SerializeField] float _camRotationSpeed;
	[SerializeField] float raycastDistance = 2f;
	CameraArm _cameraArm;
	float mouseX = 0f;
	Coroutine _coSendPacket;
	[SerializeField] GameObject _interactionObject;
	[SerializeField] Material[] _interactionMaterials;
	Material _outline;
	int _layerMask;

	protected override void Start()
	{
		base.Start();
		_cameraArm = GetComponentInChildren<CameraArm>();
		_coSendPacket = StartCoroutine(CoSyncUpdate());
		_outline = Resources.Load<Material>("Materials/Environments/DrawOutline");
		_layerMask = 1 << LayerMask.NameToLayer("Interaction");
	}

	protected override void UpdateController()
	{
		UpdateRotation();
		GetKeyInput();
		base.UpdateController();
	}

    private void FixedUpdate()
    {
		UpdateRay();
	}

    protected override void UpdateMove()
	{
		base.UpdateMove();

		if (State == CreatureState.Run)
			Pos = transform.position;
	}

	protected override void UpdateRotation()
	{
		_cameraArm.CameraRotation(_camRotationSpeed);
		mouseX += Input.GetAxis("Mouse X") * _camRotationSpeed;
		this.transform.localEulerAngles = new Vector3(0, mouseX, 0);
		Dir = new Vector3(Camera.main.transform.forward.x, 0, Camera.main.transform.forward.z);
	}

	void GetKeyInput()
	{
		InputBit = Managers.Input.SetKeyInput(KeyCode.W, InputBit, () => { CheckUpdatedFlag(); });
		InputBit = Managers.Input.SetKeyInput(KeyCode.A, InputBit, () => { CheckUpdatedFlag(); });
		InputBit = Managers.Input.SetKeyInput(KeyCode.S, InputBit, () => { CheckUpdatedFlag(); });
		InputBit = Managers.Input.SetKeyInput(KeyCode.D, InputBit, () => { CheckUpdatedFlag(); });
	}

	void UpdateRay()
	{
		Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
		RaycastHit hitInfo;

		if (Physics.Raycast(ray, out hitInfo, raycastDistance, _layerMask))
		{
			if (hitInfo.transform.gameObject == _interactionObject)
				return;

			_interactionObject = hitInfo.transform.gameObject;
			_interactionMaterials = _interactionObject.GetComponent<Renderer>().materials;
			_interactionMaterials = AddMaterial(_interactionMaterials, _outline);
			_interactionObject.GetComponent<Renderer>().materials = _interactionMaterials;

			Debug.Log("Raycast hit: " + hitInfo.transform.name);
		}
        else
        {
			if (_interactionObject != null)
			{
				_interactionMaterials = RemoveMaterial(_interactionMaterials, _outline);
				_interactionObject.GetComponent<Renderer>().materials = _interactionMaterials;
				_interactionObject = null;
			}
		}

		Debug.DrawRay(ray.origin, ray.direction * raycastDistance, Color.red);
	}

	Material[] AddMaterial(Material[] materials, Material materialToAdd)
	{
		Material[] newMaterials = new Material[materials.Length + 1];
		materials.CopyTo(newMaterials, 0);
		newMaterials[newMaterials.Length - 1] = materialToAdd;
		return newMaterials;
	}

	Material[] RemoveMaterial(Material[] materials, Material materialToRemove)
	{
		List<Material> materialList = new List<Material>(materials);
		materialList.Remove(materialToRemove);
		return materialList.ToArray();
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