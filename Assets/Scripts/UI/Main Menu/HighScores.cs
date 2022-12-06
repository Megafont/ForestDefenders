using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions.Must;

public struct HighScoreData
{
    public string Name;
    public int Rank_Score;
    public int Rank_SurvivalTime;
    public int Score;
    public float SurvivalTime;

    
    public static HighScoreData EmptyHighScoreData = new HighScoreData()
    {
        Name = "", // NOTE: This must have an underscore rather than space since spaces are used as the delimiter when we save the score data as a string in PlayerPrefs.
        Score = 0,
        SurvivalTime = 0,
        Rank_Score = -1,
        Rank_SurvivalTime = -1,
    };
}


public enum HighScoreTypes
{
    Score = 0,
    SurvivalTime,
}


public static class HighScores
{
    public const int MAX_HIGHSCORES_TABLE_SIZE = 20;
    public const string EMPTY_SLOT_STRING = "EMPTY SLOT - EMPTY SLOT - EMPTY SLOT - EMPTY SLOT - EMPTY SLOT";
    private const char SCORE_DATA_SEPERATOR_CHAR = '¶'; // This is just an arbitrary character than no one is likely to use when they enter their name.
                                                        // NOTE: If you change this value, you MUST clear the high scores data from PlayerPrefs by calling the ClearAllHighScores() method, otherwise it will crash when it tries to load the highscores data.
                                                        //       This is because that data still contains the previous separator character. There is currently no method to update the high scores data to a different separator character since there is no need for one.



    public static string TimeValueToString(float time, bool showHours = true)
    {
        int hours = Mathf.FloorToInt(time / 3600f);
        time -= hours * 3600f;

        int minutes = Mathf.FloorToInt(time / 60f);

        int seconds = Mathf.FloorToInt(time - (minutes * 60));


        if (showHours)
            return $"{hours:00}:{minutes:00}:{seconds:00}";
        else
            return $"{minutes:00}:{seconds:00}";
    }

    public static void RegisterHighScore(string name, int score, float survivialTime)
    {
        if (name == null)
            throw new Exception("The passed in name is null!");


        HighScoreData scoreData = new HighScoreData() { Name = name, Score = score, SurvivalTime = survivialTime };


        List<HighScoreData> highScoresTable = GetHighScoresTable(HighScoreTypes.Score);
        List<HighScoreData> highTimesTable = GetHighScoresTable(HighScoreTypes.SurvivalTime);

        UpdateHighScoresTable(highScoresTable, scoreData, HighScoreTypes.Score);
        UpdateHighScoresTable(highTimesTable, scoreData, HighScoreTypes.SurvivalTime);
    }
    
    public static bool IsNewHighScore(int score, float survivalTime)
    {
        string lastScoreKey = $"HighScore {HighScoreTypes.Score} {MAX_HIGHSCORES_TABLE_SIZE - 1}";
        string lastSurvivalTimeKey = $"HighScore {HighScoreTypes.SurvivalTime} {MAX_HIGHSCORES_TABLE_SIZE - 1}";

        string lastScoreData = PlayerPrefs.GetString(lastScoreKey);
        string lastSurvivalTimeData = PlayerPrefs.GetString(lastSurvivalTimeKey);


        // If the last slot in either table is the empty slot string, then we know for sure this score is a new high score since the table isn't full yet.
        if (lastScoreData == EMPTY_SLOT_STRING || lastSurvivalTimeData == EMPTY_SLOT_STRING)
            return true;


        int lastHighScore = ConvertStringToHighScoreData(lastScoreData).Score;
        float lastHighSurvivalTime = ConvertStringToHighScoreData(lastSurvivalTimeData).SurvivalTime;

        Debug.Log($"Score: {score}    Last High Score: {lastHighScore}    Survival Time: {survivalTime}    Last Survival Time: {lastHighSurvivalTime}");
        if (score > lastHighScore ||
            survivalTime > lastHighSurvivalTime)
        {
            return true;
        }

        return false;
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
                HighScoreData data = HighScoreData.EmptyHighScoreData;
                data.Rank_Score = i + 1;
                data.Rank_SurvivalTime = i + 1;
                highScoresTable.Add(data);
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

            PlayerPrefs.SetString(key, ConvertHighScoreDataToString(curScore));
            
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


        // If the new score isn't good enough to get into the high scores table, simply return.
        int lastScoreIndex = highScoresTable.Count - 1;        
        HighScoreData scoreData = highScoresTable[lastScoreIndex];
        if (!string.IsNullOrEmpty(scoreData.Name)) // Is the last table slot NOT an empty slot?
        {
            if ((tableType == HighScoreTypes.Score && newScoreData.Score <= scoreData.Score) ||  // If this is the high scores table, is the score less than the last entry in the table?
                (tableType == HighScoreTypes.SurvivalTime && newScoreData.SurvivalTime <= scoreData.SurvivalTime)) // If this is the high survival times table, is the score less than the last entry in the table?
            {
                return false;
            }
        }


        for (int i = 0; i < MAX_HIGHSCORES_TABLE_SIZE; i++)
        {
            HighScoreData curScore = highScoresTable[i];

            //if (i > 0)
            //{
                // Is this the correct place to insert the new high score into the table?
                if (string.IsNullOrEmpty(curScore.Name) || // Is this an empty slot?
                    (tableType == HighScoreTypes.Score && newScoreData.Score > highScoresTable[i].Score) || // If this is the high scores table, is this score higher than the one in the current row?
                    (tableType == HighScoreTypes.SurvivalTime && newScoreData.SurvivalTime > highScoresTable[i].SurvivalTime)) // If this is the high survival times table, is this time higher than the one in the current row?
                {
                    highScoresTable.Insert(i, newScoreData);
                    break;
                }
            //}

        } // end for i


        CalculateRanks(highScoresTable, tableType);


        // If necessary, remove the entry that got bumped off the list.
        if (highScoresTable.Count > MAX_HIGHSCORES_TABLE_SIZE)
            highScoresTable.RemoveAt(highScoresTable.Count - 1);


        SaveHighScoresTable(highScoresTable, tableType);

        return true;
    }

    private static HighScoreData ConvertStringToHighScoreData(string scoreRecord)
    {
        if (string.IsNullOrWhiteSpace(scoreRecord) || scoreRecord == EMPTY_SLOT_STRING)
            return HighScoreData.EmptyHighScoreData;


        string[] recordParts = scoreRecord.Split(SCORE_DATA_SEPERATOR_CHAR, StringSplitOptions.None);

        if (recordParts.Length != 5)
            throw new Exception($"The passed in score record string \"{scoreRecord}\" does not contain the correct number of parameters!");

        HighScoreData data = new HighScoreData();
        data.Name = recordParts[0];
        data.Score = int.Parse(recordParts[1]);
        data.SurvivalTime = float.Parse(recordParts[2]);
        data.Rank_Score = int.Parse(recordParts[3]);
        data.Rank_SurvivalTime = int.Parse(recordParts[4]);


        return data;
    }

    private static string ConvertHighScoreDataToString(HighScoreData data)
    {
        char sep = SCORE_DATA_SEPERATOR_CHAR;

        return $"{data.Name}{sep}{data.Score}{sep}{data.SurvivalTime}{sep}{data.Rank_Score}{sep}{data.Rank_SurvivalTime}";
    }


    private static void CalculateRanks(List<HighScoreData> highScoresTable, HighScoreTypes tableType)
    {
        string scoreType = tableType.ToString();

        if (highScoresTable == null)
            throw new ArgumentException($"The passed in high {scoreType}s table is null!");
        if (highScoresTable.Count == 0)
            throw new ArgumentException($"The passed in high {scoreType}s table is empty!");


        int rank = 1;
        for (int i = 0; i < MAX_HIGHSCORES_TABLE_SIZE; i++)
        {
            HighScoreData curScore = highScoresTable[i];
            if (i > 0)
            {
                if ((tableType == HighScoreTypes.Score && curScore.Score < highScoresTable[i - 1].Score) ||
                    (tableType == HighScoreTypes.SurvivalTime && curScore.SurvivalTime < highScoresTable[i - 1].SurvivalTime))
                {
                    rank++;
                }
            }


            // Save the rank for this score into the table.
            if (tableType == HighScoreTypes.Score)
                curScore.Rank_Score = rank;
            else if (tableType == HighScoreTypes.SurvivalTime)
                curScore.Rank_SurvivalTime = rank;

            highScoresTable[i] = curScore;


        } // end for i

    }

    public static void ClearAllHighScores()
    {
        PlayerPrefs.DeleteAll();        

        for (int i = 0; i < MAX_HIGHSCORES_TABLE_SIZE; i++)
        {
            string scoreKey = $"HighScore {HighScoreTypes.Score} {MAX_HIGHSCORES_TABLE_SIZE - 1}";
            string survivalTimeKey = $"HighScore {HighScoreTypes.SurvivalTime} {MAX_HIGHSCORES_TABLE_SIZE - 1}";

            PlayerPrefs.SetString(scoreKey, EMPTY_SLOT_STRING);
            PlayerPrefs.SetString(survivalTimeKey, EMPTY_SLOT_STRING);

        } // end for i

    }

    public static void ResetHighScoresTable()
    {
        ClearAllHighScores();


        // Create a list of fake high scores for players to try and beat.
        // TODO: Finalize this list with 20 items and more realistic numbers later.
        RegisterHighScore("Michael", 1000000, 3600);
        RegisterHighScore("Sam", 1000, 500);
        RegisterHighScore("Fred", 75, 200);
        RegisterHighScore("Mark", 50, 150);
        RegisterHighScore("Jack", 25, 120);
        RegisterHighScore("Fred", 10, 60);
        RegisterHighScore("Judy", 10000, 250);
        RegisterHighScore("Mark", 7500, 300);
        RegisterHighScore("Susan", 10000, 400);
        RegisterHighScore("Rachel", 12000, 300);

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
                    (tableType == HighScoreTypes.SurvivalTime && curScore.SurvivalTime < highScoresTable[i - 1].SurvivalTime))
                {
                    rank++;
                }
            }

            Debug.Log($"#{rank}    {curScore.Name}    {curScore.Score}    {TimeValueToString(curScore.SurvivalTime)}");

        } // end for

        Debug.Log(separator);

    }


}
