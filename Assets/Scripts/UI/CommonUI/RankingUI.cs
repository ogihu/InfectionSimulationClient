using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class RankingUI : MonoBehaviour
{
    Transform _content;
    TMP_Text _titleText;

    private void Awake()
    {
        _content = Util.FindChildByName(gameObject, "Content").transform;
        _titleText = Util.FindChildByName(gameObject, "TitleText").GetComponent<TMP_Text>();
    }

    public void SetRanks(S_Rank rankPacket)
    {
        if (rankPacket.Scores.Count <= 0)
            return;

        var sortedScores = rankPacket.Scores
            .OrderByDescending(score => score.FinalScore) // 1. 점수 높은 순서
            .ThenBy(score => score.GameDate);             // 2. Date 빠른 순서

        int rank = 1;

        _titleText.text = $"Ranking  -  {rankPacket.Scores[0].Position}";

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

    public void SetRanks(S_PlayerRank rankPacket)
    {
        if (rankPacket.Scores.Count <= 0)
            return;

        var scores = rankPacket.Scores
            .OrderByDescending(score => score.FinalScore) // 1. 점수 높은 순서
            .ThenBy(score => score.GameDate);             // 2. Date 빠른 순서

        Dictionary<int, (ScoreInfo, int)> ranks = new Dictionary<int, (ScoreInfo, int)>();

        // 순위 초기화
        int rank = 1;

        foreach (var score in scores)
        {
            ranks.Add(score.ScoreId, (score, rank++));
        }

        // 타이틀 설정
        _titleText.text = $"Ranking  -  {rankPacket.Scores[0].Position}";

        // 새로운 Ranking UI 생성
        Ranking rankUI = Managers.UI.CreateUI("Ranking", _content).GetComponent<Ranking>();

        // 순위에 맞는 점수 정보를 UI에 설정
        rankUI.SetRank(ranks[rankPacket.GameId].Item2, ranks[rankPacket.GameId].Item1);
        ranks.Remove(rankPacket.GameId);

        foreach (var score in ranks.Values)
        {
            // 새로운 Ranking UI 생성
            Ranking newRankUI = Managers.UI.CreateUI("Ranking", _content).GetComponent<Ranking>();

            // 순위에 맞는 점수 정보를 UI에 설정
            newRankUI.SetRank(score.Item2, score.Item1);
        }
    }
}
