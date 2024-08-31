using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

class PacketHandler
{
    public static void S_LeaveGameHandler(PacketSession session, IMessage packet)
    {
        S_LeaveGame leaveGameHandler = (S_LeaveGame)packet;
        Managers.Object.Clear();
    }

    public static void S_SpawnHandler(PacketSession session, IMessage packet)
    {
        S_Spawn spawnPacket = (S_Spawn)packet;
        foreach (ObjectInfo obj in spawnPacket.Objects)
        {
            Managers.Scene.AddWaitEvent(() => { Managers.Object.Add(obj, myPlayer: false); });
        }
    }

    public static void S_MoveHandler(PacketSession session, IMessage packet)
    {
        S_Move movePacket = (S_Move)packet;

        GameObject go = Managers.Object.FindById(movePacket.ObjectId);
        if (go == null)
            return;

        if (Managers.Object.MyPlayer.ObjectId == movePacket.ObjectId)
            return;

        BaseController bc = go.GetComponent<BaseController>();
        if (bc == null)
            return;

        bc.MoveInfo = movePacket.MoveInfo;
    }

    public static void S_DespawnHandler(PacketSession session, IMessage packet)
    {
        S_Despawn despawnPacket = (S_Despawn)packet;

        foreach (int id in despawnPacket.ObjectIds)
        {
            Managers.Object.Remove(id);
        }
    }

    public static void S_EnterGameHandler(PacketSession session, IMessage packet)
    {
        S_EnterGame enterGamePacket = (S_EnterGame)packet;
        Managers.Scene.LoadSceneWait(Define.Scene.Game);
        Managers.Scene.AddWaitEvent(() => { Managers.Object.Add(enterGamePacket.Player, myPlayer: true); });
    }

    public static void S_StartScenarioHandler(PacketSession session, IMessage packet)
    {
        S_StartScenario scenarioPacket = (S_StartScenario)packet;

        Managers.Scenario.StartScenario(scenarioPacket.ScenarioName);
    }
    
    public static void S_TalkHandler(PacketSession session, IMessage packet)
    {
        S_Talk talkPacket = (S_Talk)packet;

        GameObject go = Managers.Object.FindById(talkPacket.Id);
        if (go == null)
            return;

        if (Managers.Object.MyPlayer.ObjectId == talkPacket.Id)
            return;

        BaseController bc = go.GetComponent<BaseController>();
        if (bc == null)
            return;

        Managers.UI.ChangeChatBubble(bc.transform, talkPacket.Message);
    }

    public static void S_UnEquipHandler(PacketSession session, IMessage packet)
    {
        S_UnEquip unEquipPacket = (S_UnEquip)packet;

        GameObject go = Managers.Object.FindById(unEquipPacket.Id);
        if (go == null)
            return;

        if (Managers.Object.MyPlayer.ObjectId == unEquipPacket.Id)
            return;

        BaseController bc = go.GetComponent<BaseController>();
        if (bc == null)
            return;

        Equipment eqm = bc.Equipment[unEquipPacket.ItemName].GetComponent<Equipment>();
        if (eqm != null)
            eqm.UnEquip(bc);
    }

    public static void S_EquipHandler(PacketSession session, IMessage packet)
    {
        S_Equip equipPacket = (S_Equip)packet;
        
        GameObject go = Managers.Object.FindById(equipPacket.Id);
        if (go == null)
            return;

        if (Managers.Object.MyPlayer.ObjectId == equipPacket.Id)
            return;

        BaseController bc = go.GetComponent<BaseController>();
        if (bc == null)
            return;

        Managers.Resource.Instantiate($"Equipments/{equipPacket.ItemName}").GetComponent<Equipment>().Equip(bc);
    }

    public static void S_NextProgressHandler(PacketSession session, IMessage packet)
    {
        S_NextProgress progressPacket = (S_NextProgress)packet;

        Managers.Scenario.NextProgress(progressPacket.Progress);
    }

    public static void S_SyncHandler(PacketSession session, IMessage packet)
    {
        S_Sync syncPacket = (S_Sync)packet;

        GameObject go = Managers.Object.FindById(syncPacket.ObjectId);
        if (go == null)
            return;

        if (Managers.Object.MyPlayer.ObjectId == syncPacket.ObjectId)
            return;

        BaseController bc = go.GetComponent<BaseController>();
        if (bc == null)
            return;

        bc.PosInfo = syncPacket.PosInfo;
        bc.UpdateSync();
    }
}