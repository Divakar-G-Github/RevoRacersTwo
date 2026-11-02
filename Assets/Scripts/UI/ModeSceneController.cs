using UnityEngine;
using UnityEngine.UI;

public class ModeSceneController : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button _backButton;
    [SerializeField] private Button _timeTrialButton;
    [SerializeField] private Button _hotLapButton;
    [SerializeField] private Button _dailyChallengeButton;

    [Header("Scene Loader")]
    [SerializeField] private SceneLoader _sceneLoader;

    private void OnEnable()
    {
        _backButton.onClick.AddListener(OnBackClicked);
        _timeTrialButton.onClick.AddListener(OnTimeTrialClicked);
        _hotLapButton.onClick.AddListener(OnHotLapClicked);
        _dailyChallengeButton.onClick.AddListener(OnDailyChallengeClicked);
    }

    private void OnDisable()
    {
        _backButton.onClick.RemoveListener(OnBackClicked);
        _timeTrialButton.onClick.RemoveListener(OnTimeTrialClicked);
        _hotLapButton.onClick.RemoveListener(OnHotLapClicked);
        _dailyChallengeButton.onClick.RemoveListener(OnDailyChallengeClicked);
    }

    private void OnBackClicked()
    {
        _sceneLoader.LoadScene("Menu");
    }

    private void OnTimeTrialClicked()
    {
        RaceSession.Mode = GameMode.TimeTrial;
        RaceSession.TrackIndex = 0;

        _sceneLoader.LoadScene("TrackSelection");
    }

    private void OnHotLapClicked()
    {
        RaceSession.Mode = GameMode.HotLap;
        RaceSession.TrackIndex = 0;

        _sceneLoader.LoadScene("TrackSelection");
    }

    private void OnDailyChallengeClicked()
    {
        RaceSession.Mode = GameMode.Challenge;

        Debug.Log("Daily Challenge - Coming Soon!");
    }
}
