using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System;
using System.Collections.Generic;

class PacketManager
{
	#region Singleton
	static PacketManager _instance = new PacketManager();
	public static PacketManager Instance { get { return _instance; } }
	#endregion

	PacketManager()
	{
		Register();
	}

	Dictionary<ushort, Action<PacketSession, ArraySegment<byte>, ushort>> _onRecv = new Dictionary<ushort, Action<PacketSession, ArraySegment<byte>, ushort>>();
	Dictionary<ushort, Action<PacketSession, IMessage>> _handler = new Dictionary<ushort, Action<PacketSession, IMessage>>();
		
	public Action<PacketSession, IMessage, ushort> CustomHandler { get; set; }

	public void Register()
	{		
		_onRecv.Add((ushort)MsgId.SEnterGame, MakePacket<S_EnterGame>);
		_handler.Add((ushort)MsgId.SEnterGame, PacketHandler.S_EnterGameHandler);		
		_onRecv.Add((ushort)MsgId.SLeaveGame, MakePacket<S_LeaveGame>);
		_handler.Add((ushort)MsgId.SLeaveGame, PacketHandler.S_LeaveGameHandler);		
		_onRecv.Add((ushort)MsgId.SSpawn, MakePacket<S_Spawn>);
		_handler.Add((ushort)MsgId.SSpawn, PacketHandler.S_SpawnHandler);		
		_onRecv.Add((ushort)MsgId.SDespawn, MakePacket<S_Despawn>);
		_handler.Add((ushort)MsgId.SDespawn, PacketHandler.S_DespawnHandler);		
		_onRecv.Add((ushort)MsgId.SMove, MakePacket<S_Move>);
		_handler.Add((ushort)MsgId.SMove, PacketHandler.S_MoveHandler);		
		_onRecv.Add((ushort)MsgId.SSync, MakePacket<S_Sync>);
		_handler.Add((ushort)MsgId.SSync, PacketHandler.S_SyncHandler);		
		_onRecv.Add((ushort)MsgId.SStartScenario, MakePacket<S_StartScenario>);
		_handler.Add((ushort)MsgId.SStartScenario, PacketHandler.S_StartScenarioHandler);		
		_onRecv.Add((ushort)MsgId.SNextProgress, MakePacket<S_NextProgress>);
		_handler.Add((ushort)MsgId.SNextProgress, PacketHandler.S_NextProgressHandler);		
		_onRecv.Add((ushort)MsgId.STalk, MakePacket<S_Talk>);
		_handler.Add((ushort)MsgId.STalk, PacketHandler.S_TalkHandler);		
		_onRecv.Add((ushort)MsgId.SEquip, MakePacket<S_Equip>);
		_handler.Add((ushort)MsgId.SEquip, PacketHandler.S_EquipHandler);		
		_onRecv.Add((ushort)MsgId.SUnEquip, MakePacket<S_UnEquip>);
		_handler.Add((ushort)MsgId.SUnEquip, PacketHandler.S_UnEquipHandler);		
		_onRecv.Add((ushort)MsgId.SRegistAccount, MakePacket<S_RegistAccount>);
		_handler.Add((ushort)MsgId.SRegistAccount, PacketHandler.S_RegistAccountHandler);		
		_onRecv.Add((ushort)MsgId.SLogin, MakePacket<S_Login>);
		_handler.Add((ushort)MsgId.SLogin, PacketHandler.S_LoginHandler);		
		_onRecv.Add((ushort)MsgId.SRank, MakePacket<S_Rank>);
		_handler.Add((ushort)MsgId.SRank, PacketHandler.S_RankHandler);		
		_onRecv.Add((ushort)MsgId.SPlayerRank, MakePacket<S_PlayerRank>);
		_handler.Add((ushort)MsgId.SPlayerRank, PacketHandler.S_PlayerRankHandler);
	}

	public void OnRecvPacket(PacketSession session, ArraySegment<byte> buffer)
	{
		ushort count = 0;

		ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
		count += 2;
		ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
		count += 2;

		Action<PacketSession, ArraySegment<byte>, ushort> action = null;
		if (_onRecv.TryGetValue(id, out action))
			action.Invoke(session, buffer, id);
	}

	void MakePacket<T>(PacketSession session, ArraySegment<byte> buffer, ushort id) where T : IMessage, new()
	{
		T pkt = new T();
		pkt.MergeFrom(buffer.Array, buffer.Offset + 4, buffer.Count - 4);

		if (CustomHandler != null)
		{
			CustomHandler.Invoke(session, pkt, id);
		}
		else
		{
			Action<PacketSession, IMessage> action = null;
			if (_handler.TryGetValue(id, out action))
				action.Invoke(session, pkt);
		}
	}

	public Action<PacketSession, IMessage> GetPacketHandler(ushort id)
	{
		Action<PacketSession, IMessage> action = null;
		if (_handler.TryGetValue(id, out action))
			return action;
		return null;
	}
}