
using UnityEngine;
using UnityEngine.UI;

public class TrackSelectionController : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button _backButton;
    [SerializeField] private Button _trackButton;

    [Header("Scene Loader")]
    [SerializeField] private SceneLoader _sceneLoader;

    private void OnEnable()
    {
        _backButton.onClick.AddListener(OnBackClicked);
        _trackButton.onClick.AddListener(OnTrackClicked);
    }

    private void OnDisable()
    {
        _backButton.onClick.RemoveListener(OnBackClicked);
        _trackButton.onClick.RemoveListener(OnTrackClicked);
    }

    private void OnBackClicked()
    {
        _sceneLoader.LoadScene("Mode");
    }

    private void OnTrackClicked()
    {
        _sceneLoader.LoadScene("Race");
    }
}
