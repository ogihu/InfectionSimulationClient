using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using UnityEngine;

public class ObjectManager
{
    public MyPlayerController MyPlayer { get; set; }

    Dictionary<int, GameObject> _objects = new Dictionary<int, GameObject>();
	public Dictionary<int, GameObject> Objects { get { return _objects; } }

	List<GameObject> _players = new List<GameObject>();
	public List<GameObject> Players { get { return _players; } }

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
				_players.Add(go);
				go.name = $"{info.UserInfo.Name}";

				MyPlayer = go.GetComponent<MyPlayerController>();
				MyPlayer.ObjectId = info.ObjectId;
				MyPlayer.UserInfo = info.UserInfo;
				MyPlayer.MoveInfo = info.MoveInfo;
				MyPlayer.PosInfo = info.PosInfo;
				MyPlayer.ImmediateSync();
			}
			else
			{
				GameObject go = Managers.Resource.Instantiate($"Creatures/Player/{info.UserInfo.Position}");
				_objects.Add(info.ObjectId, go);
				_players.Add(go);
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

	public void RemovePlayer(GameObject player)
    {
		if(_players.Contains(player))
			_players.Remove(player);
    }

	public GameObject FindById(int id)
	{
		GameObject go = null;
		_objects.TryGetValue(id, out go);
		return go;
	}

	public GameObject Find(Func<GameObject, bool> condition)
	{
		foreach (GameObject obj in _objects.Values)
		{
			if (condition.Invoke(obj))
				return obj;
		}

		return null;
	}

	public void Clear()
	{
		foreach (GameObject obj in _objects.Values)
			Managers.Resource.Destroy(obj);

		_objects.Clear();
		_players.Clear();

        MyPlayer = null;
    }
}
