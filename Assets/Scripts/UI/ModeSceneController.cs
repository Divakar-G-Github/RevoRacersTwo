
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ModeSceneController : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button _backButton;
    [SerializeField] private Button _timeTrailButton;

    [Header("Scene Loader")]
    [SerializeField] private SceneLoader _sceneLoader;

    private void OnEnable()
    {
        _backButton.onClick.AddListener(OnBackClicked);
        _timeTrailButton.onClick.AddListener(OnTimeTrailClicked);
    }

    private void OnDisable()
    {
        _backButton.onClick.RemoveListener(OnBackClicked);
        _timeTrailButton.onClick.RemoveListener(OnTimeTrailClicked);
    }

    private void OnBackClicked()
    {
        _sceneLoader.LoadScene("Menu");
    }

    private void OnTimeTrailClicked()
    {
        _sceneLoader.LoadScene("TrackSelection");
    }
}
