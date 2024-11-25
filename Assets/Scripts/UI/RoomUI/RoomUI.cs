using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomUI : MonoBehaviour
{
    Transform _changePositionGroup;
    List<ChangePositionButton> _buttons = new List<ChangePositionButton>();

    private void Awake()
    {
        _changePositionGroup = Util.FindChildByName(gameObject, "ChangePositionGroup").GetComponent<Transform>();
        
        for(int i = 0; i < _changePositionGroup.childCount; i++)
        {
            _buttons.Add(_changePositionGroup.GetChild(i).GetComponent<ChangePositionButton>());
            _buttons[i].Index = i;
        }
    }

    public void SendStart()
    {
        C_StartSimulation startPacket = new C_StartSimulation();
        Managers.Network.Send(startPacket);
    }

    public void SetPlayersPosition(PositionMatching[] playerPositions)
    {
        if (playerPositions.Length <= 0)
            return;

        Dictionary<int, string> playerDict = new Dictionary<int, string>();
        foreach (var player in playerPositions)
        {
            playerDict[player.Index] = player.PlayerId;
        }

        for (int i = 0; i < _buttons.Count; i++)
        {
            if (playerDict.ContainsKey(i))
            {
                // 플레이어가 있으면 해당 ID로 텍스트 설정
                _buttons[i].SetText(playerDict[i]);
            }
            else
            {
                // 플레이어가 없으면 "없음"으로 텍스트 설정
                _buttons[i].SetText("없음");
            }
        }
    }
}
