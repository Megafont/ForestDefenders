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
