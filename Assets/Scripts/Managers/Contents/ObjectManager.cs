using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ObjectManager
{
	public MyPlayerController MyPlayer { get; set; }

	Dictionary<int, GameObject> _objects = new Dictionary<int, GameObject>();
	public Dictionary<string, BaseController> Characters = new Dictionary<string, BaseController>();

	public static GameObjectType GetObjectTypeById(int id)
	{
		int type = (id >> 24) & 0x7F;
		return (GameObjectType)type;
	}

	public void ChangeModel(BaseController bc, string modelName)
    {
		MyPlayerController mpc = bc as MyPlayerController;
		if(mpc != null)
        {
			GameObject go = Managers.Resource.Instantiate($"Creatures/MyPlayer/{modelName}");
			MyPlayerController newMyPlayer = go.GetComponent<MyPlayerController>();
			go.transform.position = bc.transform.position;
			go.transform.rotation = bc.transform.rotation;
			bc.CopyTo(newMyPlayer);
			GameObject oldChar = _objects[mpc.ObjectId];
			_objects[newMyPlayer.ObjectId] = go;
			MyPlayer = newMyPlayer;
			Managers.Resource.Destroy(oldChar);

			return;
		}

		PlayerController pc = bc as PlayerController;
		if(pc != null)
        {
			GameObject go = Managers.Resource.Instantiate($"Creatures/Player/{modelName}");
			PlayerController newPlayer = go.GetComponent<PlayerController>();
			go.transform.position = bc.transform.position;
			go.transform.rotation = bc.transform.rotation;
			bc.CopyTo(newPlayer);
			GameObject oldChar = _objects[pc.ObjectId];
			_objects[newPlayer.ObjectId] = go;
			Managers.Resource.Destroy(oldChar);

			return;
		}

		NPCController npc = bc as NPCController;
		if (npc != null)
		{
			GameObject go = Managers.Resource.Instantiate($"Creatures/NPC/{modelName}");
			NPCController newNPC = go.GetComponent<NPCController>();
			go.transform.position = bc.transform.position;
			go.transform.rotation = bc.transform.rotation;
			bc.CopyTo(newNPC);
			Managers.Scenario.NPCs[npc.Position] = newNPC;
			GameObject oldChar = bc.gameObject;
			Managers.Resource.Destroy(oldChar);

			return;
		}

		Managers.UI.DestroyBubble(bc.transform);
	}

	public void Add(ObjectInfo info, bool myPlayer = false)
	{
		GameObjectType objectType = GetObjectTypeById(info.ObjectId);

		if (objectType == GameObjectType.Player)
		{
			if (myPlayer)
			{
				GameObject go = Managers.Resource.Instantiate($"Creatures/MyPlayer/{info.UserInfo.Position}");
				_objects.Add(info.ObjectId, go);
				go.name = $"{info.UserInfo.AccountId}";

				MyPlayer = go.GetComponent<MyPlayerController>();
				MyPlayer.ObjectId = info.ObjectId;
				MyPlayer.UserInfo = info.UserInfo;
				MyPlayer.MoveInfo = info.MoveInfo;
				MyPlayer.PosInfo = info.PosInfo;
				MyPlayer.ImmediateSync();

				Characters.Add(MyPlayer.Position, MyPlayer);

				Managers.Scenario.UpdateScenarioAssist("시나리오 시작을 기다리는 중 입니다...", MyPlayer.UserInfo.Position);
			}
			else
			{
				GameObject go = Managers.Resource.Instantiate($"Creatures/Player/{info.UserInfo.Position}");
				_objects.Add(info.ObjectId, go);
				go.name = $"{info.UserInfo.AccountId}";

				PlayerController pc = go.GetComponent<PlayerController>();
				pc.ObjectId = info.ObjectId;
				pc.UserInfo = info.UserInfo;
				pc.MoveInfo = info.MoveInfo;
				pc.PosInfo = info.PosInfo;
				pc.ImmediateSync();

				Characters.Add(pc.Position, pc);
			}
		}
	}

	public void Remove(int id)
	{
		GameObject go = FindById(id);
		if (go == null)
			return;

		_objects.Remove(id);
		if (Characters.TryGetValue(go.GetComponent<BaseController>().Position, out BaseController bc))
		{

		}
		Managers.Resource.Destroy(go);
	}

	public GameObject FindById(int id)
	{
		GameObject go = null;
		_objects.TryGetValue(id, out go);
		return go;
	}

	public void Clear()
	{
		foreach (GameObject obj in _objects.Values)
		{ 
			if(obj != null)
			{
                obj.GetComponent<PlayerController>().Clear();
                Managers.Resource.Destroy(obj);
            }
		}

		_objects.Clear();
		Characters.Clear();
		MyPlayer = null;
	}

	public GameObject FindPosition(string position)
	{
		foreach (var obj in _objects.Values)
		{
			if (obj.GetComponent<BaseController>().Position == position)
				return obj;
		}

		return null;
	}
}
