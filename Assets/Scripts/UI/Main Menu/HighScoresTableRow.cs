using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using TMPro;


public class HighScoresTableRow : MonoBehaviour
{

    [SerializeField] private TMP_Text RankText;
    [SerializeField] private TMP_Text NameText;
    [SerializeField] private TMP_Text ScoreText;
    [SerializeField] private TMP_Text TimeText;



    // Start is called before the first frame update
    void Start()
    {
        if (RankText == null)
            throw new Exception("The rank text object is not set in the inspector!");
        if (NameText == null)
            throw new Exception("The score text object is not set in the inspector!");
        if (ScoreText == null)
            throw new Exception("The score text object is not set in the inspector!");
        if (TimeText == null)
            throw new Exception("The time text object is not set in the inspector!");
    }


    public void SetRowData(HighScoreData scoreData, HighScoreTypes tableType)
    {
        if (tableType == HighScoreTypes.Score)
            RankText.text = $"#{scoreData.Rank_Score}";
        else if (tableType == HighScoreTypes.SurvivalTime)
            RankText.text = $"#{scoreData.Rank_SurvivalTime}";

        if (string.IsNullOrEmpty(scoreData.Name))
            NameText.text = "EMPTY SLOT";
        else
            NameText.text = scoreData.Name;

        ScoreText.text = scoreData.Score.ToString();
        TimeText.text = HighScores.TimeValueToString(scoreData.SurvivalTime);
    }
}
