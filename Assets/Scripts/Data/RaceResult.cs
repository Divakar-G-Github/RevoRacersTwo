using System.Collections.Generic;

// The result of a finished race. Later this is what you send to the leaderboard.
public class RaceResult
{
    public float TotalTime { get; }
    public float BestLapTime { get; }
    public IReadOnlyList<float> LapTimes { get; }

    public RaceResult(float totalTime, List<float> lapTimes)
    {
        TotalTime = totalTime;
        LapTimes = new List<float>(lapTimes);   // a copy, so later changes can't affect it

        float best = float.MaxValue;
        foreach (float lapTime in lapTimes)
        {
            if (lapTime < best)
            {
                best = lapTime;
            }
        }

        BestLapTime = best;
    }
}