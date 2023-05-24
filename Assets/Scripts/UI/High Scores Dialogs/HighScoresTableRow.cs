using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using TMPro;


public class HighScoresTableRow : MonoBehaviour
{
    [SerializeField] private TMP_Text _RankText;
    [SerializeField] private TMP_Text _NameText;
    [SerializeField] private TMP_Text _ScoreText;
    [SerializeField] private TMP_Text _TimeText;



    // Start is called before the first frame update
    void Start()
    {
        if (_RankText == null)
            throw new Exception("The rank text object is not set in the inspector!");
        if (_NameText == null)
            throw new Exception("The score text object is not set in the inspector!");
        if (_ScoreText == null)
            throw new Exception("The score text object is not set in the inspector!");
        if (_TimeText == null)
            throw new Exception("The time text object is not set in the inspector!");
    }

    public void SetRowData(HighScoreData scoreData, HighScoreTypes tableType)
    {
        if (tableType == HighScoreTypes.Score)
            _RankText.text = $"#{scoreData.Rank_Score}";
        else if (tableType == HighScoreTypes.SurvivalTime)
            _RankText.text = $"#{scoreData.Rank_SurvivalTime}";

        if (string.IsNullOrEmpty(scoreData.Name))
            _NameText.text = "EMPTY SLOT";
        else
            _NameText.text = scoreData.Name;

        _ScoreText.text = scoreData.Score.ToString("n0");
        _TimeText.text = Utils_HighScores.TimeValueToString(scoreData.SurvivalTime);
    }
}
