using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsPanel : MonoBehaviour
{
    private const int MinUsernameLength = 3;

    private static readonly Vector2Int[] Resolutions =
    {
        new Vector2Int(1280, 720),
        new Vector2Int(1600, 900),
        new Vector2Int(1920, 1080),
        new Vector2Int(2560, 1440)
    };

    [Header("References")]
    [SerializeField] private UIManager _uiManager;
    [SerializeField] private ConfirmPopup _confirmPopup;

    [Header("Tabs")]
    [SerializeField] private Toggle _audioTab;
    [SerializeField] private Toggle _graphicsTab;
    [SerializeField] private Toggle _accountTab;
    [SerializeField] private GameObject _audioPanel;
    [SerializeField] private GameObject _graphicsPanel;
    [SerializeField] private GameObject _accountPanel;

    [Header("Audio")]
    [SerializeField] private Slider _masterSlider;
    [SerializeField] private Slider _musicSlider;
    [SerializeField] private Slider _sfxSlider;
    [SerializeField] private TMP_Text _masterValueText;
    [SerializeField] private TMP_Text _musicValueText;
    [SerializeField] private TMP_Text _sfxValueText;
    [SerializeField] private Toggle _muteToggle;

    [Header("Graphics")]
    [SerializeField] private TMP_Dropdown _qualityDropdown;
    [SerializeField] private TMP_Dropdown _resolutionDropdown;
    [SerializeField] private Toggle _fullscreenToggle;

    [Header("Account")]
    [SerializeField] private TMP_InputField _usernameInput;
    [SerializeField] private TMP_InputField _emailInput;
    [SerializeField] private TMP_InputField _bioInput;
    [SerializeField] private TMP_Text _accountErrorText;

    [Header("Buttons")]
    [SerializeField] private Button _applyButton;
    [SerializeField] private Button _resetButton;
    [SerializeField] private Button _closeButton;

    private SettingsData _workingData;

    // ---------- Lifecycle ----------

    private void Awake()
    {
        FillQualityOptions();
        FillResolutionOptions();
    }

    private void OnEnable()
    {
        _workingData = SettingsStorage.Load();
        RefreshUI();
        OpenAudioTab();
        AddListeners();
    }

    private void OnDisable()
    {
        RemoveListeners();
    }

    private void AddListeners()
    {
        _audioTab.onValueChanged.AddListener(OnAudioTabChanged);
        _graphicsTab.onValueChanged.AddListener(OnGraphicsTabChanged);
        _accountTab.onValueChanged.AddListener(OnAccountTabChanged);

        _masterSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        _musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        _sfxSlider.onValueChanged.AddListener(OnSfxVolumeChanged);
        _muteToggle.onValueChanged.AddListener(OnMuteChanged);

        _qualityDropdown.onValueChanged.AddListener(OnQualityChanged);
        _resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
        _fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);

        _usernameInput.onValueChanged.AddListener(OnUsernameChanged);
        _emailInput.onValueChanged.AddListener(OnEmailChanged);
        _bioInput.onValueChanged.AddListener(OnBioChanged);

        _applyButton.onClick.AddListener(OnApplyClicked);
        _resetButton.onClick.AddListener(OnResetClicked);
        _closeButton.onClick.AddListener(OnCloseClicked);
    }

    private void RemoveListeners()
    {
        _audioTab.onValueChanged.RemoveListener(OnAudioTabChanged);
        _graphicsTab.onValueChanged.RemoveListener(OnGraphicsTabChanged);
        _accountTab.onValueChanged.RemoveListener(OnAccountTabChanged);

        _masterSlider.onValueChanged.RemoveListener(OnMasterVolumeChanged);
        _musicSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);
        _sfxSlider.onValueChanged.RemoveListener(OnSfxVolumeChanged);
        _muteToggle.onValueChanged.RemoveListener(OnMuteChanged);

        _qualityDropdown.onValueChanged.RemoveListener(OnQualityChanged);
        _resolutionDropdown.onValueChanged.RemoveListener(OnResolutionChanged);
        _fullscreenToggle.onValueChanged.RemoveListener(OnFullscreenChanged);

        _usernameInput.onValueChanged.RemoveListener(OnUsernameChanged);
        _emailInput.onValueChanged.RemoveListener(OnEmailChanged);
        _bioInput.onValueChanged.RemoveListener(OnBioChanged);

        _applyButton.onClick.RemoveListener(OnApplyClicked);
        _resetButton.onClick.RemoveListener(OnResetClicked);
        _closeButton.onClick.RemoveListener(OnCloseClicked);
    }

    // ---------- Fill UI from data ----------

    private void RefreshUI()
    {
        _masterSlider.SetValueWithoutNotify(_workingData.MasterVolume);
        _musicSlider.SetValueWithoutNotify(_workingData.MusicVolume);
        _sfxSlider.SetValueWithoutNotify(_workingData.SfxVolume);
        _muteToggle.SetIsOnWithoutNotify(_workingData.IsMuted);

        _qualityDropdown.SetValueWithoutNotify(_workingData.QualityIndex);
        _resolutionDropdown.SetValueWithoutNotify(_workingData.ResolutionIndex);
        _fullscreenToggle.SetIsOnWithoutNotify(_workingData.IsFullscreen);

        _usernameInput.SetTextWithoutNotify(_workingData.Username);
        _emailInput.SetTextWithoutNotify(_workingData.Email);
        _bioInput.SetTextWithoutNotify(_workingData.Bio);

        UpdateVolumeLabels();
        UpdateSliderInteractable();
        ValidateAccount();
    }

    private void UpdateVolumeLabels()
    {
        _masterValueText.text = Mathf.RoundToInt(_workingData.MasterVolume).ToString();
        _musicValueText.text = Mathf.RoundToInt(_workingData.MusicVolume).ToString();
        _sfxValueText.text = Mathf.RoundToInt(_workingData.SfxVolume).ToString();
    }

    private void UpdateSliderInteractable()
    {
        bool canChangeVolume = !_workingData.IsMuted;
        _masterSlider.interactable = canChangeVolume;
        _musicSlider.interactable = canChangeVolume;
        _sfxSlider.interactable = canChangeVolume;
    }

    // ---------- Tabs ----------

    private void OpenAudioTab()
    {
        _audioTab.SetIsOnWithoutNotify(true);
        _audioPanel.SetActive(true);
        _graphicsPanel.SetActive(false);
        _accountPanel.SetActive(false);
    }

    private void OnAudioTabChanged(bool isOn) { _audioPanel.SetActive(isOn); }
    private void OnGraphicsTabChanged(bool isOn) { _graphicsPanel.SetActive(isOn); }
    private void OnAccountTabChanged(bool isOn) { _accountPanel.SetActive(isOn); }

    // ---------- Audio handlers ----------

    private void OnMasterVolumeChanged(float value)
    {
        _workingData.MasterVolume = value;
        UpdateVolumeLabels();
    }

    private void OnMusicVolumeChanged(float value)
    {
        _workingData.MusicVolume = value;
        UpdateVolumeLabels();
    }

    private void OnSfxVolumeChanged(float value)
    {
        _workingData.SfxVolume = value;
        UpdateVolumeLabels();
    }

    private void OnMuteChanged(bool isMuted)
    {
        _workingData.IsMuted = isMuted;
        UpdateSliderInteractable();
    }

    // ---------- Graphics handlers ----------

    private void OnQualityChanged(int index) { _workingData.QualityIndex = index; }
    private void OnResolutionChanged(int index) { _workingData.ResolutionIndex = index; }
    private void OnFullscreenChanged(bool isOn) { _workingData.IsFullscreen = isOn; }

    // ---------- Dropdown options ----------

    private void FillQualityOptions()
    {
        _qualityDropdown.ClearOptions();
        _qualityDropdown.AddOptions(new List<string>(QualitySettings.names));
    }

    private void FillResolutionOptions()
    {
        List<string> options = new List<string>();
        foreach (Vector2Int resolution in Resolutions)
        {
            options.Add($"{resolution.x} x {resolution.y}");
        }
        _resolutionDropdown.ClearOptions();
        _resolutionDropdown.AddOptions(options);
    }

    // ---------- Account ----------

    private void OnUsernameChanged(string text)
    {
        _workingData.Username = text;
        ValidateAccount();
    }

    private void OnEmailChanged(string text)
    {
        _workingData.Email = text;
        ValidateAccount();
    }

    private void OnBioChanged(string text) { _workingData.Bio = text; }

    private void ValidateAccount()
    {
        string error = GetAccountError(_workingData.Username, _workingData.Email);
        bool isValid = string.IsNullOrEmpty(error);

        _applyButton.interactable = isValid;
        _accountErrorText.text = error;
        _accountErrorText.gameObject.SetActive(!isValid);
    }

    private static string GetAccountError(string username, string email)
    {
        if (username.Trim().Length < MinUsernameLength)
        {
            return $"Username must be at least {MinUsernameLength} characters.";
        }
        if (!email.Contains("@"))
        {
            return "Email must contain '@'.";
        }
        return string.Empty;
    }

    // ---------- Apply / Reset / Close ----------

    private void OnApplyClicked()
    {
        SettingsStorage.Save(_workingData);

        AudioListener.volume = _workingData.IsMuted ? 0f : _workingData.MasterVolume / 100f;
        QualitySettings.SetQualityLevel(_workingData.QualityIndex, true);

        Vector2Int resolution = Resolutions[_workingData.ResolutionIndex];
        FullScreenMode mode = _workingData.IsFullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
        Screen.SetResolution(resolution.x, resolution.y, mode);
    }

    private void OnResetClicked()
    {
        _confirmPopup.Show("Reset audio and graphics settings to default?", ResetToDefaults);
    }

    private void ResetToDefaults()
    {
        SettingsData defaults = SettingsData.CreateDefault();
        defaults.Username = _workingData.Username; // keep the account details
        defaults.Email = _workingData.Email;
        defaults.Bio = _workingData.Bio;

        _workingData = defaults;
        RefreshUI(); // the player presses Apply to save
    }

    private void OnCloseClicked() { _uiManager.Back(); }
}