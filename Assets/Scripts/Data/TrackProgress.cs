using UnityEngine;

// Saves which maps are unlocked and the best time on each map (PlayerPrefs).
public static class TrackProgress
{
    private const string HighestUnlockedKey = "Tracks.HighestUnlocked";
    private const string BestTimeKeyPrefix = "Tracks.BestTime.";   // + map index, for example "Tracks.BestTime.0"

    // Map 1 (index 0) is always unlocked.
    public static bool IsUnlocked(int trackIndex)
    {
        return trackIndex <= PlayerPrefs.GetInt(HighestUnlockedKey, 0);
    }

    // 0 means no time yet.
    public static float GetBestTime(int trackIndex)
    {
        return PlayerPrefs.GetFloat(BestTimeKeyPrefix + trackIndex, 0f);
    }

    // Keeps the best time, and unlocks the next map if the time is under unlockTime.
    // Returns true if a new map was unlocked.
    public static bool CompleteTrack(int trackIndex, float totalTime, float unlockTime)
    {
        float best = GetBestTime(trackIndex);
        if (best <= 0f || totalTime < best)
        {
            PlayerPrefs.SetFloat(BestTimeKeyPrefix + trackIndex, totalTime);
        }

        bool unlockedNew = false;
        if (totalTime < unlockTime)
        {
            int next = trackIndex + 1;
            if (next > PlayerPrefs.GetInt(HighestUnlockedKey, 0))
            {
                PlayerPrefs.SetInt(HighestUnlockedKey, next);
                unlockedNew = true;
            }
        }

        PlayerPrefs.Save();
        return unlockedNew;
    }

    // For demos.
    public static void UnlockAll(int trackCount)
    {
        PlayerPrefs.SetInt(HighestUnlockedKey, Mathf.Max(0, trackCount - 1));
        PlayerPrefs.Save();
    }

    public static void ResetAll(int trackCount)
    {
        PlayerPrefs.DeleteKey(HighestUnlockedKey);
        for (int i = 0; i < trackCount; i++)
        {
            PlayerPrefs.DeleteKey(BestTimeKeyPrefix + i);
        }

        PlayerPrefs.Save();
    }
}