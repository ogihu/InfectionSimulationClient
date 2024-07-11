using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ObjectManager
{
	public MyPlayerController MyPlayer { get; set; }

	Dictionary<int, GameObject> _objects = new Dictionary<int, GameObject>();

	public static GameObjectType GetObjectTypeById(int id)
	{
		int type = (id >> 24) & 0x7F;
		return (GameObjectType)type;
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
				go.name = $"{info.UserInfo.Name}";

				MyPlayer = go.GetComponent<MyPlayerController>();
				MyPlayer.ObjectId = info.ObjectId;
				MyPlayer.UserInfo = info.UserInfo;
				MyPlayer.MoveInfo = info.MoveInfo;
				MyPlayer.PosInfo = info.PosInfo;
				MyPlayer.ImmediateSync();
				Managers.Scenario.UpdateScenarioAssist("시나리오 시작을 기다리는 중 입니다...", MyPlayer.UserInfo.Position);
			}
			else
			{
				GameObject go = Managers.Resource.Instantiate($"Creatures/Player/{info.UserInfo.Position}");
				_objects.Add(info.ObjectId, go);
				go.name = $"{info.UserInfo.Name}";

				PlayerController pc = go.GetComponent<PlayerController>();
				pc.ObjectId = info.ObjectId;
				pc.UserInfo = info.UserInfo;
				pc.MoveInfo = info.MoveInfo;
				pc.PosInfo = info.PosInfo;
				pc.ImmediateSync();
			}
		}
	}

	public void Remove(int id)
	{
		GameObject go = FindById(id);
		if (go == null)
			return;

		_objects.Remove(id);
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
			Managers.Resource.Destroy(obj);

		_objects.Clear();

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
