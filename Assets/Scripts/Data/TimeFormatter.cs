using UnityEngine;

// Turns seconds into text like 01:23.456
public static class TimeFormatter
{
    public static string Format(float seconds)
    {
        int totalMilliseconds = Mathf.FloorToInt(Mathf.Max(0f, seconds) * 1000f);
        int minutes = totalMilliseconds / 60000;
        int secs = (totalMilliseconds / 1000) % 60;
        int milliseconds = totalMilliseconds % 1000;

        return $"{minutes:00}:{secs:00}.{milliseconds:000}";
    }
}