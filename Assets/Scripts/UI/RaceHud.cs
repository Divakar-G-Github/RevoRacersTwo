using System.Collections;
using TMPro;
using UnityEngine;

// Shows the lap, the running time, the last lap time and the 3-2-1-GO countdown.
public class RaceHud : MonoBehaviour
{
    [SerializeField] private RaceManager _race;
    [SerializeField] private TMP_Text _timeText;
    [SerializeField] private TMP_Text _lapText;
    [SerializeField] private TMP_Text _lastLapText;        // optional
    [SerializeField] private TMP_Text _countdownText;
    [SerializeField] private float _goVisibleTime = 0.8f;

    private void OnEnable()
    {
        _race.CountdownTick += OnCountdownTick;
        _race.LapCompleted += OnLapCompleted;

        _countdownText.gameObject.SetActive(false);
        if (_lastLapText != null)
        {
            _lastLapText.text = string.Empty;
        }

        ShowLap();
    }

    private void OnDisable()
    {
        _race.CountdownTick -= OnCountdownTick;
        _race.LapCompleted -= OnLapCompleted;
    }

    private void Update()
    {
        _timeText.text = TimeFormatter.Format(_race.ElapsedTime);
    }

    private void OnCountdownTick(int seconds)
    {
        _countdownText.gameObject.SetActive(true);
        _countdownText.text = seconds > 0 ? seconds.ToString() : "GO!";

        if (seconds == 0)
        {
            StartCoroutine(HideCountdownAfterDelay());
        }
    }

    private IEnumerator HideCountdownAfterDelay()
    {
        yield return new WaitForSeconds(_goVisibleTime);
        _countdownText.gameObject.SetActive(false);
    }

    private void OnLapCompleted(int lapNumber, float lapTime)
    {
        if (_lastLapText != null)
        {
            _lastLapText.text = $"LAP {lapNumber}   {TimeFormatter.Format(lapTime)}";
        }

        ShowLap();
    }

    private void ShowLap()
    {
        _lapText.text = $"LAP {_race.CurrentLap}/{_race.TotalLaps}";
    }
}