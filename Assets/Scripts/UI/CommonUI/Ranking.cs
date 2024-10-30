using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Ranking : MonoBehaviour
{
    TMP_Text _rank;
    TMP_Text _player;
    TMP_Text _id;
    TMP_Text _score;
    TMP_Text _date;

    private void Awake()
    {
        _rank = Util.FindChildByName(gameObject, "Rank").GetComponent<TMP_Text>();
        _player = Util.FindChildByName(gameObject, "Player").GetComponent<TMP_Text>();
        _id = Util.FindChildByName(gameObject, "Id").GetComponent<TMP_Text>();
        _score = Util.FindChildByName(gameObject, "Score").GetComponent<TMP_Text>();
        _date = Util.FindChildByName(gameObject, "Date").GetComponent<TMP_Text>();
    }

    public void SetRank(int rank, ScoreInfo info)
    {
        _rank.text = rank.ToString();
        _player.text = info.PlayerName;
        _id.text = info.AccountId;
        _score.text = info.FinalScore.ToString();

        // 날짜 설정 (Ticks를 DateTime으로 변환한 후, 년/월/일만 표시)
        DateTime gameDate = new DateTime(info.GameDate);
        _date.text = gameDate.ToString("yyyy-MM-dd");
    }
}
