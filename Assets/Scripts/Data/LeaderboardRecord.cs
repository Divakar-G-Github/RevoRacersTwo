public class LeaderboardRecord
{
    public string PlayerName { get; }
    public float TimeSeconds { get; }

    public LeaderboardRecord(string playerName, float timeSeconds)
    {
        PlayerName = playerName;
        TimeSeconds = timeSeconds;
    }
}