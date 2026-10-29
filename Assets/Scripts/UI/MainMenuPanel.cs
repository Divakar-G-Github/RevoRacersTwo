using UnityEngine;
using UnityEngine.UI;

public class MainMenuPanel : MonoBehaviour
{
    [SerializeField] private UIManager _uiManager;
    [SerializeField] private SceneLoader _sceneLoader;

    [Header("Buttons")]
    [SerializeField] private Button _playButton;
    [SerializeField] private Button _settingsButton;
    [SerializeField] private Button _settingsIconButton;
    [SerializeField] private Button _shopButton;
    [SerializeField] private Button _creditsButton;
    [SerializeField] private Button _leaderBoardButton;
    [SerializeField] private Button _profileButton;
    [SerializeField] private Button _quitButton;
    [SerializeField] private string _modeSceneName = "Mode";

    private void OnEnable()
    {
        _playButton.onClick.AddListener(OnPlayClicked);
        _settingsButton.onClick.AddListener(OnSettingsClicked);
        _settingsIconButton.onClick.AddListener(OnSettingsClicked);
        _shopButton.onClick.AddListener(OnShopClicked);
        _creditsButton.onClick.AddListener(OnCreditsClicked);
        _leaderBoardButton.onClick.AddListener(OnLeaderBoardClicked);
        _profileButton.onClick.AddListener(OnProfileClicked);
        _quitButton.onClick.AddListener(OnQuitClicked);
    }

    private void OnDisable()
    {
        _playButton.onClick.RemoveListener(OnPlayClicked);
        _settingsButton.onClick.RemoveListener(OnSettingsClicked);
        _settingsIconButton.onClick.RemoveListener(OnSettingsClicked);
        _shopButton.onClick.RemoveListener(OnShopClicked);
        _creditsButton.onClick.RemoveListener(OnCreditsClicked);
        _leaderBoardButton.onClick.RemoveListener(OnLeaderBoardClicked);
        _profileButton.onClick.RemoveListener(OnProfileClicked);
        _quitButton.onClick.RemoveListener(OnQuitClicked);
    }

    private void OnPlayClicked()
    {
        Debug.Log("Play pressed - loading the Mode scene.");
        _sceneLoader.LoadScene("Mode");
    }
    private void OnSettingsClicked() { _uiManager.Show(ScreenType.Settings); }
    private void OnShopClicked() { _uiManager.Show(ScreenType.Shop); }
    private void OnCreditsClicked() { _uiManager.Show(ScreenType.Credits); }
    private void OnLeaderBoardClicked() { _uiManager.Show(ScreenType.LeaderBoard); }
    private void OnProfileClicked() { _uiManager.Show(ScreenType.Profile); }

    private void OnQuitClicked()
    {
#if UNITY_EDITOR
        Debug.Log("Quit pressed. Application.Quit() is ignored in the Editor.");
#endif
        Application.Quit();
    }
}