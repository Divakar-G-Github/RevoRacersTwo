using UnityEngine;
using UnityEngine.UI;

// Connects the Mode screen buttons. Time Trial and Hot Lap open the map selection.
public class ModePanel : MonoBehaviour
{
    [SerializeField] private UIManager _uiManager;

    [Header("Buttons")]
    [SerializeField] private Button _timeTrialButton;
    [SerializeField] private Button _hotLapButton;
    [SerializeField] private Button _challengeButton;

    private void OnEnable()
    {
        _timeTrialButton.onClick.AddListener(OnTimeTrialClicked);
        _hotLapButton.onClick.AddListener(OnHotLapClicked);
        _challengeButton.onClick.AddListener(OnChallengeClicked);
    }

    private void OnDisable()
    {
        _timeTrialButton.onClick.RemoveListener(OnTimeTrialClicked);
        _hotLapButton.onClick.RemoveListener(OnHotLapClicked);
        _challengeButton.onClick.RemoveListener(OnChallengeClicked);
    }

    private void OnTimeTrialClicked()
    {
        OpenTrackSelection(GameMode.TimeTrial);
    }

    private void OnHotLapClicked()
    {
        OpenTrackSelection(GameMode.HotLap);
    }

    private void OnChallengeClicked()
    {
        Debug.Log("Challenge is coming soon.");
    }

    // Remembers the mode, so the track screen knows which scene to load.
    private void OpenTrackSelection(GameMode mode)
    {
        RaceSession.Mode = mode;
        _uiManager.Show(ScreenType.TrackSelection);
    }
}