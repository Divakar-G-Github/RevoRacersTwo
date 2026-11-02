
using UnityEngine;
using UnityEngine.UI;

public class TrackSelectionController : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button _backButton;
    [SerializeField] private Button _playButton;

    [Header("Scripts")]
    [SerializeField] private TrackSelectionPanel _trackSelectionPanel;
    [SerializeField] private SceneLoader _sceneLoader;

    private void OnEnable()
    {
        _backButton.onClick.AddListener(OnBackClicked);
        _playButton.onClick.AddListener(OnPlayClicked);
    }

    private void OnDisable()
    {
        _backButton.onClick.RemoveListener(OnBackClicked);
        _playButton.onClick.RemoveListener(OnPlayClicked);
    }

    private void OnBackClicked()
    {
        _sceneLoader.LoadScene("Mode");
    }

    private void OnPlayClicked()
    {
        string sceneName = _trackSelectionPanel.GetSelectedSceneName();

        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("No valid scene selected.");
            return;
        }

        Debug.Log($"Loading {RaceSession.Mode} scene: {sceneName}");

        _sceneLoader.LoadScene(sceneName);
    }
}
