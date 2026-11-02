// The kinds of race the player can start from the Mode screen.
public enum GameMode
{
    TimeTrial,
    HotLap,
    Challenge
}

// Remembers the player's choices while moving between screens and scenes.
// "static" means there is one shared copy that survives loading a new scene.
public static class RaceSession
{
    public static GameMode Mode { get; set; } = GameMode.TimeTrial;
    public static int TrackIndex { get; set; }
}