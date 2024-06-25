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
				GameObject go = Managers.Resource.Instantiate($"Creatures/MyPlayer/FemaleDoctor");
				_objects.Add(info.ObjectId, go);
				_players.Add(go);
				go.name = $"{info.Name}";

				MyPlayer = go.GetComponent<MyPlayerController>();
				MyPlayer.Id = info.ObjectId;
				MyPlayer.Name = info.Name;
				MyPlayer.MoveInfo = info.MoveInfo;
				MyPlayer.PosInfo = info.PosInfo;
				MyPlayer.Sync();
			}
			else
			{
				GameObject go = Managers.Resource.Instantiate($"Creatures/Player/FemaleDoctor");
				_objects.Add(info.ObjectId, go);
				_players.Add(go);
				go.name = $"{info.Name}";

				PlayerController pc = go.GetComponent<PlayerController>();
				pc.Id = info.ObjectId;
				pc.Name = info.Name;
				pc.MoveInfo = info.MoveInfo;
				pc.PosInfo = info.PosInfo;
				pc.Sync();
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
