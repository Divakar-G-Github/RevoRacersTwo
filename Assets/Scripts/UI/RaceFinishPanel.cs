using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Shows the results when the race is over, with Restart and Main Menu buttons.
// Put this script on an object that is always active (such as the Canvas), not on the panel itself.
public class RaceFinishPanel : MonoBehaviour
{
    [SerializeField] private RaceManager _race;
    [SerializeField] private GameObject _panelRoot;        // the results panel (starts hidden)

    [Header("Texts")]
    [SerializeField] private TMP_Text _totalTimeText;
    [SerializeField] private TMP_Text _bestLapText;
    [SerializeField] private TMP_Text _lapTimesText;

    [Header("Buttons")]
    [SerializeField] private Button _restartButton;
    [SerializeField] private Button _menuButton;

    [Header("Settings")]
    [SerializeField] private float _showDelay = 1.5f;      // lets the player see the car cross the line
    [SerializeField] private string _menuSceneName = "Menu";

    private void OnEnable()
    {
        _panelRoot.SetActive(false);

        _race.RaceFinished += OnRaceFinished;
        _restartButton.onClick.AddListener(OnRestartClicked);
        _menuButton.onClick.AddListener(OnMenuClicked);
    }

    private void OnDisable()
    {
        _race.RaceFinished -= OnRaceFinished;
        _restartButton.onClick.RemoveListener(OnRestartClicked);
        _menuButton.onClick.RemoveListener(OnMenuClicked);
    }

    private void OnRaceFinished(RaceResult result)
    {
        StartCoroutine(ShowAfterDelay(result));
    }

    private IEnumerator ShowAfterDelay(RaceResult result)
    {
        yield return new WaitForSeconds(_showDelay);

        _totalTimeText.text = $"TOTAL  {TimeFormatter.Format(result.TotalTime)}";
        _bestLapText.text = $"BEST LAP  {TimeFormatter.Format(result.BestLapTime)}";
        _lapTimesText.text = BuildLapList(result);

        _panelRoot.SetActive(true);
    }

    private static string BuildLapList(RaceResult result)
    {
        StringBuilder builder = new StringBuilder();
        for (int i = 0; i < result.LapTimes.Count; i++)
        {
            if (i > 0)
            {
                builder.Append('\n');
            }

            builder.Append("LAP ").Append(i + 1).Append("   ").Append(TimeFormatter.Format(result.LapTimes[i]));
        }

        return builder.ToString();
    }

    private void OnRestartClicked()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);   // reload this race
    }

    private void OnMenuClicked()
    {
        SceneManager.LoadScene(_menuSceneName);
    }
}