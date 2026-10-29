using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Shows the car's speed and lets the player return to the menu.
public class GameplayHud : MonoBehaviour
{
    [SerializeField] private CarController _car;
    [SerializeField] private TMP_Text _speedText;
    [SerializeField] private Button _menuButton;
    [SerializeField] private string _menuSceneName = "Menu";

    private void OnEnable()
    {
        _menuButton.onClick.AddListener(OnMenuClicked);
    }

    private void OnDisable()
    {
        _menuButton.onClick.RemoveListener(OnMenuClicked);
    }

    private void Update()
    {
        if (_car != null)
        {
            _speedText.text = $"{Mathf.RoundToInt(_car.SpeedKmh)} km/h";
        }
    }

    private void OnMenuClicked()
    {
        SceneManager.LoadScene(_menuSceneName);
    }
}