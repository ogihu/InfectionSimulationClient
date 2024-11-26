using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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

        Managers.Scenario.StartScenario(scenarioPacket.ScenarioName, scenarioPacket);
    }
    
    public static void S_TalkHandler(PacketSession session, IMessage packet)
    {
        S_Talk talkPacket = (S_Talk)packet;

        GameObject go = Managers.Object.FindById(talkPacket.Id);
        if (go == null)
            return;

        if ((Managers.Object.MyPlayer.ObjectId == talkPacket.Id) && talkPacket.TTSSelf == false)
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

        Item eqm = bc.Items[unEquipPacket.ItemName].GetComponent<Item>();

        if (eqm != null)
            eqm.UnUse();
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

        Managers.Resource.Instantiate($"Items/{equipPacket.ItemName}").GetComponent<Item>().Use(bc);
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

    public static void S_RegistAccountHandler(PacketSession session, IMessage packet)
    {
        S_RegistAccount registPacket = (S_RegistAccount)packet;
        
        if(Managers.Network.WaitingUI != null)
            Managers.UI.DestroyUI(Managers.Network.WaitingUI);

        GameObject warningUI = Managers.UI.CreateUI("WarningUI");

        switch (registPacket.Result)
        {
            case RegistAccountState.ExistPlayer:
                //이미 등록된 사용자입니다.
                warningUI.GetComponent<WarningUI>().SetText("이미 등록된 사용자입니다.");
                break;
            case RegistAccountState.ExistAccount:
                //사용 중인 아이디입니다.
                warningUI.GetComponent<WarningUI>().SetText("사용 중인 아이디입니다.");
                break;
            case RegistAccountState.RegistError:
                warningUI.GetComponent<WarningUI>().SetText("계정 등록 중 오류가 발생하였습니다.\n잠시 후 다시 시도해주세요.");
                break;
            case RegistAccountState.RegistComplete:
                //계정이 생성되었습니다.
                warningUI.GetComponent<WarningUI>().SetText("계정이 등록되었습니다.", "NOTICE");
                Managers.UI.DestroyUI(GameObject.Find("RegistUI"));
                break;
        }
    }

    public static void S_LoginHandler(PacketSession session, IMessage packet)
    {
        S_Login loginPacket = (S_Login)packet;

        if (Managers.Network.WaitingUI != null)
            Managers.UI.DestroyUI(Managers.Network.WaitingUI);

        GameObject warningUI;

        switch (loginPacket.Result)
        {
            case LoginState.NoAccount:
                // 등록되지 않은 계정입니다.
                warningUI = Managers.UI.CreateUI("WarningUI");
                warningUI.GetComponent<WarningUI>().SetText("등록되지 않은 계정입니다.");
                break;
            case LoginState.WrongPassword:
                // 비밀번호가 틀렸습니다.
                warningUI = Managers.UI.CreateUI("WarningUI");
                warningUI.GetComponent<WarningUI>().SetText("비밀번호가 틀렸습니다.");
                break;
            case LoginState.LoginComplete:
                // 로비로 이동
                Managers.Scene.LoadScene(Define.Scene.Lobby);
                break;
            case LoginState.AlreadyLogin:
                // 이미 훈련 시작됨
                warningUI = Managers.UI.CreateUI("WarningUI");
                warningUI.GetComponent<WarningUI>().SetText("이미 로그인된 계정입니다.");
                break;
            case LoginState.LoginError:
                // 로그인 중 오류 발생
                warningUI = Managers.UI.CreateUI("WarningUI");
                warningUI.GetComponent<WarningUI>().SetText("오류가 발생하였습니다.\n잠시 후 다시 시도해주세요.");
                break;
        }
    }

    public static void S_RankHandler(PacketSession session, IMessage packet)
    {
        S_Rank rankPacket = (S_Rank)packet;

        Managers.UI.CreateUI("RankingUI").GetComponent<RankingUI>().SetRanks(rankPacket);
        
        if (Managers.Network.WaitingUI != null)
            Managers.UI.DestroyUI(Managers.Network.WaitingUI);
    }

    public static void S_PlayerRankHandler(PacketSession session, IMessage packet)
    {
        S_PlayerRank rankPacket = (S_PlayerRank)packet;

        Managers.UI.CreateUI("RankingUI").GetComponent<RankingUI>().SetRanks(rankPacket);

        if (Managers.Network.WaitingUI != null)
            Managers.UI.DestroyUI(Managers.Network.WaitingUI);
    }

    public static void S_RoomListHandler(PacketSession session, IMessage packet)
    {
        S_RoomList roomPacket = (S_RoomList)packet;

        if (Managers.Scene.CurrentScene is LobbyScene lobby)
        {
            lobby.LobbyUI.UpdateRoomList(roomPacket.Rooms.ToArray());
        }
    }

    public static void S_MakeRoomHandler(PacketSession session, IMessage packet)
    {
        S_MakeRoom roomPacket = (S_MakeRoom)packet;

        GameObject warningUI;

        switch (roomPacket.Result)
        {
            case MakeRoomState.ExistTitle:
                if (Managers.Network.WaitingUI != null)
                    Managers.UI.DestroyUI(Managers.Network.WaitingUI);

                warningUI = Managers.UI.CreateUI("WarningUI");
                warningUI.GetComponent<WarningUI>().SetText("중복되는 방 제목입니다.");
                break;
            case MakeRoomState.MakeRoomError:
                if (Managers.Network.WaitingUI != null)
                    Managers.UI.DestroyUI(Managers.Network.WaitingUI);

                warningUI = Managers.UI.CreateUI("WarningUI");
                warningUI.GetComponent<WarningUI>().SetText("방 생성 중 오류가 발생하였습니다.\n다시 시도해주세요.");
                break;
            case MakeRoomState.MakeRoomComplete:
                Managers.Scene.LoadSceneWait(Define.Scene.Room);
                Managers.Scene.AddWaitEvent(() => { GameObject.Find("StartButton").SetActive(true); GameObject.Find("NPCOption").SetActive(true); });
                break;
        }
    }

    public static void S_EnterRoomHandler(PacketSession session, IMessage packet)
    {
        S_EnterRoom roomPacket = (S_EnterRoom)packet;

        GameObject warningUI;

        switch (roomPacket.Result)
        {
            case EnterRoomState.FullMembers:
                if (Managers.Network.WaitingUI != null)
                    Managers.UI.DestroyUI(Managers.Network.WaitingUI);

                warningUI = Managers.UI.CreateUI("WarningUI");
                warningUI.GetComponent<WarningUI>().SetText("인원이 가득 찼습니다.");
                break;
            case EnterRoomState.IncorrectPassword:
                if (Managers.Network.WaitingUI != null)
                    Managers.UI.DestroyUI(Managers.Network.WaitingUI);

                warningUI = Managers.UI.CreateUI("WarningUI");
                warningUI.GetComponent<WarningUI>().SetText("비밀번호가 틀렸습니다.");
                break;
            case EnterRoomState.NoRoom:
                if (Managers.Network.WaitingUI != null)
                    Managers.UI.DestroyUI(Managers.Network.WaitingUI);

                warningUI = Managers.UI.CreateUI("WarningUI");
                warningUI.GetComponent<WarningUI>().SetText("존재하지 않는 방입니다.");
                break;
            case EnterRoomState.EnterError:
                if (Managers.Network.WaitingUI != null)
                    Managers.UI.DestroyUI(Managers.Network.WaitingUI);

                warningUI = Managers.UI.CreateUI("WarningUI");
                warningUI.GetComponent<WarningUI>().SetText("입장 중 오류가 발생했습니다.");
                break;
            case EnterRoomState.EnterRoomComplete:
                Managers.Scene.LoadSceneWait(Define.Scene.Room);
                Managers.Scene.AddWaitEvent(() => { GameObject.Find("StartButton").SetActive(false); GameObject.Find("NPCOption").SetActive(false); });
                break;
        }
    }

    public static void S_MakerExitHandler(PacketSession session, IMessage packet)
    {
        Managers.UI.CreateUI("Room/MakerExitNotice");
    }

    public static void S_UpdateRoomInfoHandler(PacketSession session, IMessage packet)
    {
        S_UpdateRoomInfo roomPacket = (S_UpdateRoomInfo)packet;

        if(Managers.Scene.CurrentScene is RoomScene room)
        {
            room.RoomUI.SetPlayersPosition(roomPacket.Players.ToArray());
        }
    }
}