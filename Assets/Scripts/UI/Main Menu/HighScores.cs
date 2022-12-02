using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public struct HighScoreData
{
    public string Name;
    public int Rank_Score;
    public int Rank_Time;
    public int Score;
    public float Time;

    
    public static HighScoreData EmptyHighScoreData = new HighScoreData()
    {
        Name = "EMPTY_SLOT", // NOTE: This must have an underscore rather than space since spaces are used as the delimiter when we save the score data as a string in PlayerPrefs.
        Score = 0,
        Time = 0,
        Rank_Score = -1,
        Rank_Time = -1,
    };
}


public enum HighScoreTypes
{
    Score = 0,
    Time,
}


public static class HighScores
{
    private const int MAX_HIGHSCORES_TABLE_SIZE = 20;
    private const string EMPTY_SLOT_STRING = "EMPTY SLOT - EMPTY SLOT - EMPTY SLOT - EMPTY SLOT - EMPTY SLOT";


    
    public static string TimeValueToString(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time - (minutes * 60));

        return $"{minutes:0}:{seconds:00}";
    }

    public static float RegisterHighScore(HighScoreData scoreData)
    {
        List<HighScoreData> highScoresTable = GetHighScoresTable(HighScoreTypes.Score);
        List<HighScoreData> highTimesTable = GetHighScoresTable(HighScoreTypes.Time);

        UpdateHighScoresTable(highScoresTable, scoreData, HighScoreTypes.Score);
        UpdateHighScoresTable(highTimesTable, scoreData, HighScoreTypes.Time);

        return 0;
    }
    

    public static List<HighScoreData> GetHighScoresTable(HighScoreTypes tableType)
    {
        List<HighScoreData> highScoresTable = new List<HighScoreData>();


        for (int i = 0; i < MAX_HIGHSCORES_TABLE_SIZE; i++)
        {          
            string key = $"HighScore {tableType} {i}";

            string record = PlayerPrefs.GetString(key, EMPTY_SLOT_STRING);

            //Debug.Log($"LOADED: \"{key}\"    \"{record}\"");


            if (record != EMPTY_SLOT_STRING)
            {
                HighScoreData scoreData = ConvertStringToHighScoreData(record);
                highScoresTable.Add(scoreData);
            }
            else
            {
                highScoresTable.Add(HighScoreData.EmptyHighScoreData);
            }

        } // end for i


        return highScoresTable;
    }

    private static void SaveHighScoresTable(List<HighScoreData> highScoresTable, HighScoreTypes tableType)
    {
        if (highScoresTable == null)
            throw new ArgumentException($"The passed in high scores table is null!");
        if (highScoresTable.Count == 0)
            throw new ArgumentException($"The passed in high scores table is empty!");


        for (int i = 0; i < MAX_HIGHSCORES_TABLE_SIZE; i++)
        {
            HighScoreData curScore = highScoresTable[i];

            string key = $"HighScore {tableType} {i}";

            //Debug.Log($"SAVED: \"{key}\"    \"{HighScoreDataToString(curScore)}\"");

            PlayerPrefs.SetString(key, HighScoreDataToString(curScore));
            
        }

    }

    /// <summary>
    /// Updates the passed in high scores table.
    /// </summary>
    /// <param name="highScoresTable">The high scores table to update.</param>
    /// <param name="newScoreData">The new score data to add to the table if it is good enough.</param>
    /// <returns>True if the new score made it into the high scores table, or false otherwise.</returns>
    /// <exception cref="ArgumentException">If the passed in highScoresTable is null or empty.</exception>
    private static bool UpdateHighScoresTable(List<HighScoreData> highScoresTable, HighScoreData newScoreData, HighScoreTypes tableType)
    {
        if (highScoresTable == null)
            throw new ArgumentException($"The passed in high scores table is null!");
        if (highScoresTable.Count == 0)
            throw new ArgumentException($"The passed in high scores table is empty!");



        // If the new score isn't good enough to get into the high scores table, so simply return.
        int lastScoreIndex = highScoresTable.Count - 1;
        if (newScoreData.Score <= highScoresTable[lastScoreIndex].Score)
            return false;


        for (int i = 0; i <= MAX_HIGHSCORES_TABLE_SIZE; i++)
        {
            if (i > 0)
            {
                if ((tableType == HighScoreTypes.Score && newScoreData.Score > highScoresTable[i - 1].Score) ||
                    (tableType == HighScoreTypes.Time && newScoreData.Time > highScoresTable[i - 1].Time))
                {
                    highScoresTable.Insert(i - 1, newScoreData);
                    break;
                }
            }    
            
        } // end for i


        // If necessary, remove the entry that got bumped off the list.
        if (highScoresTable.Count > MAX_HIGHSCORES_TABLE_SIZE)
            highScoresTable.RemoveAt(highScoresTable.Count - 1);


        SaveHighScoresTable(highScoresTable, tableType);

        return true;
    }

    private static HighScoreData ConvertStringToHighScoreData(string scoreRecord)
    {
        if (scoreRecord == EMPTY_SLOT_STRING)
            return HighScoreData.EmptyHighScoreData;


        string[] recordParts = scoreRecord.Split(' ', StringSplitOptions.None);

        if (recordParts.Length != 3)
            throw new Exception("The passed in score record string does not contain the correct number of parameters!");

        HighScoreData data = new HighScoreData();
        data.Name = recordParts[0];
        //data.Rank_Score = int.Parse(recordParts[1]);
        //data.Rank_Time = int.Parse(recordParts[2]);
        data.Score = int.Parse(recordParts[1]);
        data.Time = float.Parse(recordParts[2]);


        return data;
    }

    private static string HighScoreDataToString(HighScoreData data)
    {
        return $"{data.Name} {data.Score} {data.Time}";
    }


    public static void DEBUG_LogHighScoresTable(List<HighScoreData> highScoresTable, HighScoreTypes tableType)
    {
        string scoreType = tableType.ToString();

        if (highScoresTable == null)
            throw new ArgumentException($"The passed in high {scoreType}s table is null!");
        if (highScoresTable.Count == 0)
            throw new ArgumentException($"The passed in high {scoreType}s table is empty!");


        scoreType = scoreType.ToUpper();
        string separator = new string('-', 256);


        // Print the record scores table to the Unity console.
        Debug.Log(separator);
        Debug.Log($"HIGH {scoreType}S TABLE:");
        
        Debug.Log(separator);


        int rank = 1;        
        for (int i = 0; i < MAX_HIGHSCORES_TABLE_SIZE; i++)
        {
            HighScoreData curScore = highScoresTable[i];
            if (i > 0)
            {
                if ((tableType == HighScoreTypes.Score && curScore.Score < highScoresTable[i - 1].Score) ||
                    (tableType == HighScoreTypes.Time && curScore.Time < highScoresTable[i - 1].Time))
                {
                    rank++;
                }
            }

            Debug.Log($"#{rank}    {curScore.Name}    {curScore.Score}    {TimeValueToString(curScore.Time)}");

        } // end for

        Debug.Log(separator);

    }


}
