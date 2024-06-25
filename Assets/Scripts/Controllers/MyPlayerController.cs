using Google.Protobuf.Protocol;
using UnityEngine;

public class MyPlayerController : PlayerController
{
	protected override void UpdateController()
	{
		GetKeyInput();
		base.UpdateController();
	}

	void GetKeyInput()
	{
		InputBit = Managers.Input.SetKeyInput(KeyCode.W, InputBit, () => { SendSync(); });
		InputBit = Managers.Input.SetKeyInput(KeyCode.A, InputBit, () => { SendSync(); });
		InputBit = Managers.Input.SetKeyInput(KeyCode.S, InputBit, () => { SendSync(); });
		InputBit = Managers.Input.SetKeyInput(KeyCode.D, InputBit, () => { SendSync(); });

		CheckUpdatedFlag();
	}

	private void SendSync()
    {
		Pos = transform.position;
		C_Sync syncPacket = new C_Sync();
		syncPacket.PosInfo = PosInfo;
		Managers.Network.Send(syncPacket);
	}

	private void CheckUpdatedFlag()
	{
		if (_updated)
		{
			C_Move movePacket = new C_Move();
			movePacket.MoveInfo = MoveInfo;
			Managers.Network.Send(movePacket);
			_updated = false;
		}
	}
}