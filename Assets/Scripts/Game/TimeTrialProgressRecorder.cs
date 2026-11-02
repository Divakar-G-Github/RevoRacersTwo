
using UnityEngine;

public class TimeTrialProgressRecorder : MonoBehaviour
{
    [SerializeField] private RaceManager _race;
    [SerializeField] private TrackList _tracks;

    private void OnEnable()
    {
        _race.RaceFinished += OnRaceFinished;
    }

    private void OnDisable()
    {
        _race.RaceFinished -= OnRaceFinished;
    }

    private void OnRaceFinished(RaceResult result)
    {
        if (RaceSession.Mode != GameMode.TimeTrial)
        {
            return;
        }

        int index = RaceSession.TrackIndex;

        if (index < 0 || index >= _tracks.Count)
        {
            return;
        }

        float targetTime = _tracks.GetTrack(index).UnlockNextTime;

        bool unlockedNewMap = TrackProgress.CompleteTrack(
            index,
            result.TotalTime,
            targetTime
        );

        if (unlockedNewMap)
        {
            Debug.Log($"Congratulations! Map {index + 2} unlocked!");
        }
        else
        {
            Debug.Log($"Target time not achieved. Required: {targetTime} seconds.");
        }
    }
}
