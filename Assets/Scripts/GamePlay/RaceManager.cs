using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RaceState
{
    Countdown,
    Racing,
    Finished
}

// Runs the race: countdown, timer, ordered checkpoints, laps and the finish.
public class RaceManager : MonoBehaviour
{
    [Header("Race")]
    [SerializeField] private int _totalLaps = 2;
    [SerializeField] private int _countdownSeconds = 3;   // 0 = start the timer at once

    [Header("References")]
    [SerializeField] private CarInput _carInput;
    [SerializeField] private CheckpointTrigger _finishLine;
    [SerializeField] private CheckpointTrigger[] _checkpoints;   // in driving order

    private readonly List<float> _lapTimes = new List<float>();
    private int _nextCheckpoint;
    private float _raceStartTime;
    private float _lapStartTime;
    private float _finalTime;

    public event Action<int> CountdownTick;           // 3, 2, 1, then 0 = GO
    public event Action<int, float> LapCompleted;     // lap number, lap time
    public event Action<RaceResult> RaceFinished;

    public RaceState State { get; private set; } = RaceState.Countdown;
    public int TotalLaps => _totalLaps;
    public int CurrentLap { get; private set; } = 1;

    public float ElapsedTime
    {
        get
        {
            switch (State)
            {
                case RaceState.Racing:
                    return Time.time - _raceStartTime;
                case RaceState.Finished:
                    return _finalTime;
                default:
                    return 0f;
            }
        }
    }

    private void OnEnable()
    {
        _finishLine.Crossed += OnTriggerCrossed;
        foreach (CheckpointTrigger checkpoint in _checkpoints)
        {
            checkpoint.Crossed += OnTriggerCrossed;
        }
    }

    private void OnDisable()
    {
        _finishLine.Crossed -= OnTriggerCrossed;
        foreach (CheckpointTrigger checkpoint in _checkpoints)
        {
            checkpoint.Crossed -= OnTriggerCrossed;
        }
    }

    private void Start()
    {
        if (_checkpoints.Length == 0)
        {
            Debug.LogWarning("RaceManager: add at least one checkpoint, or the finish line alone counts a lap.", this);
        }

        StartCoroutine(CountdownRoutine());
    }

    // ---------- Countdown ----------

    private IEnumerator CountdownRoutine()
    {
        State = RaceState.Countdown;
        _carInput.InputEnabled = false;   // the car is held still

        for (int seconds = _countdownSeconds; seconds > 0; seconds--)
        {
            CountdownTick?.Invoke(seconds);
            yield return new WaitForSeconds(1f);
        }

        CountdownTick?.Invoke(0);   // GO!
        _carInput.InputEnabled = true;
        _raceStartTime = Time.time;
        _lapStartTime = Time.time;
        State = RaceState.Racing;
    }

    // ---------- Checkpoints and laps ----------

    private void OnTriggerCrossed(CheckpointTrigger trigger)
    {
        if (State != RaceState.Racing)
        {
            return;
        }

        if (trigger == _finishLine)
        {
            HandleFinishLine();
            return;
        }

        // Checkpoints only count in order, so shortcuts don't work.
        if (_nextCheckpoint < _checkpoints.Length && trigger == _checkpoints[_nextCheckpoint])
        {
            _nextCheckpoint++;
        }
    }

    private void HandleFinishLine()
    {
        if (_nextCheckpoint < _checkpoints.Length)
        {
            return;   // not every checkpoint was passed: this is not a lap
        }

        float now = Time.time;
        float lapTime = now - _lapStartTime;

        _lapTimes.Add(lapTime);
        LapCompleted?.Invoke(_lapTimes.Count, lapTime);

        _lapStartTime = now;
        _nextCheckpoint = 0;

        if (_lapTimes.Count >= _totalLaps)
        {
            FinishRace(now);
        }
        else
        {
            CurrentLap = _lapTimes.Count + 1;
        }
    }

    private void FinishRace(float now)
    {
        State = RaceState.Finished;
        _finalTime = now - _raceStartTime;
        _carInput.InputEnabled = false;   // the car brakes to a stop

        RaceFinished?.Invoke(new RaceResult(_finalTime, _lapTimes));
    }

    // Right-click the component header in Play mode to test the UI without driving a lap.
    [ContextMenu("Test: Complete Lap")]
    private void TestCompleteLap()
    {
        if (!Application.isPlaying || State != RaceState.Racing)
        {
            return;
        }

        _nextCheckpoint = _checkpoints.Length;
        HandleFinishLine();
    }
}