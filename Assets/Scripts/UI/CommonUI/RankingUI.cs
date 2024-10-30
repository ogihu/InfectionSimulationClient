using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RankingUI : MonoBehaviour
{
    Transform _content;

    private void Awake()
    {
        _content = Util.FindChildByName(gameObject, "Content").transform;
    }

    public void SetRanks(S_Rank rankPacket)
    {
        if (rankPacket.Scores.Count <= 0)
            return;

        var sortedScores = rankPacket.Scores
            .OrderByDescending(score => score.FinalScore) // 1. 점수 높은 순서
            .ThenBy(score => score.FaultCount)            // 2. 실수 적은 순서
            .ThenBy(score => score.GameDate);             // 3. Date 빠른 순서

        int rank = 1;
        foreach (var score in sortedScores)
        {
            // 새로운 Ranking UI 생성
            Ranking rankUI = Managers.UI.CreateUI("Ranking", _content).GetComponent<Ranking>();

            // 순위에 맞는 점수 정보를 UI에 설정
            rankUI.SetRank(rank, score);

            // 순위를 증가
            rank++;
        }
    }
}
