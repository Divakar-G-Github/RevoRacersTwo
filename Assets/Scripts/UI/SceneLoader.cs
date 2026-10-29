
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{
    [Header("Loading UI")]
    [SerializeField] private GameObject _loadingPanel;
    [SerializeField] private Image _progressFill;
    [SerializeField] private TMP_Text _progressText;

    private bool _isLoading;

    public void LoadScene(string sceneName)
    {
        if (_isLoading) return;

        StartCoroutine(LoadSceneRoutine(sceneName));
    }


    private IEnumerator LoadSceneRoutine(string sceneName)
    {
        _isLoading = true;

        _loadingPanel.SetActive(true);
        _progressFill.fillAmount = 0f;
        _progressText.text = "0%";

        AsyncOperation operation =
            SceneManager.LoadSceneAsync(sceneName);

        operation.allowSceneActivation = false;

        float displayedProgress = 0f;
        float elapsedTime = 0f;

        // Wait for loading, animation, and minimum display time.
        while (operation.progress < 0.9f ||
               displayedProgress < 1f ||
               elapsedTime < 1.5f)
        {
            elapsedTime += Time.unscaledDeltaTime;

            float targetProgress =
                Mathf.Clamp01(operation.progress / 0.9f);

            // Smoothly animate the visible progress.
            displayedProgress = Mathf.MoveTowards(
                displayedProgress,
                targetProgress,
                Time.unscaledDeltaTime * 0.8f
            );

            _progressFill.fillAmount = displayedProgress;

            _progressText.text =
                $"{Mathf.RoundToInt(displayedProgress * 100f)}%";

            yield return null;
        }

        _progressFill.fillAmount = 1f;
        _progressText.text = "100%";

        operation.allowSceneActivation = true;
    }

}
