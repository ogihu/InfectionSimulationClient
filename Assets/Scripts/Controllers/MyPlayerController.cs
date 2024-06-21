using Google.Protobuf.Protocol;
using UnityEngine;

public class MyPlayerController : PlayerController
{
	bool _moveKeyPressed = false;

	protected override void UpdateController()
	{
		switch (State)
		{
			case CreatureState.Idle:
				GetDirInput();
				break;
			case CreatureState.Moving:
				GetDirInput();
				break;
		}

		base.UpdateController();
	}

	void GetDirInput()
	{
		_moveKeyPressed = true;

		if (Input.GetKey(KeyCode.W))
		{

		}
		else if (Input.GetKey(KeyCode.S))
		{

		}
		else if (Input.GetKey(KeyCode.A))
		{

		}
		else if (Input.GetKey(KeyCode.D))
		{

		}
		else
		{
			_moveKeyPressed = false;
		}
	}

	protected override void UpdateIdle()
	{
		if (_moveKeyPressed)
		{
			State = CreatureState.Moving;
			return;
		}
	}

	protected override void MoveToNextPos()
	{
		if (_moveKeyPressed == false)
		{
			State = CreatureState.Idle;
			CheckUpdatedFlag();
			return;
		}

		Vector3 destPos = Pos;

		//키 입력에 따라 방향별로 destPos 증가

		CheckUpdatedFlag();
	}

	private void CheckUpdatedFlag()
	{
		if (_updated)
		{
			C_Move movePacket = new C_Move();
			movePacket.PosInfo = PosInfo;
			Managers.Network.Send(movePacket);
			_updated = false;
		}
	}
}