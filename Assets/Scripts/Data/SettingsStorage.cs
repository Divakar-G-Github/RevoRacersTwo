using UnityEngine;

public static class SettingsStorage
{
    private const string MasterVolumeKey = "Settings.MasterVolume";
    private const string MusicVolumeKey = "Settings.MusicVolume";
    private const string SfxVolumeKey = "Settings.SfxVolume";
    private const string MutedKey = "Settings.Muted";
    private const string QualityKey = "Settings.Quality";
    private const string ResolutionKey = "Settings.Resolution";
    private const string FullscreenKey = "Settings.Fullscreen";
    private const string UsernameKey = "Settings.Username";
    private const string EmailKey = "Settings.Email";
    private const string BioKey = "Settings.Bio";

    public static SettingsData Load()
    {
        SettingsData defaults = SettingsData.CreateDefault();
        return new SettingsData
        {
            MasterVolume = PlayerPrefs.GetFloat(MasterVolumeKey, defaults.MasterVolume),
            MusicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, defaults.MusicVolume),
            SfxVolume = PlayerPrefs.GetFloat(SfxVolumeKey, defaults.SfxVolume),
            IsMuted = PlayerPrefs.GetInt(MutedKey, defaults.IsMuted ? 1 : 0) == 1,
            QualityIndex = PlayerPrefs.GetInt(QualityKey, defaults.QualityIndex),
            ResolutionIndex = PlayerPrefs.GetInt(ResolutionKey, defaults.ResolutionIndex),
            IsFullscreen = PlayerPrefs.GetInt(FullscreenKey, defaults.IsFullscreen ? 1 : 0) == 1,
            Username = PlayerPrefs.GetString(UsernameKey, defaults.Username),
            Email = PlayerPrefs.GetString(EmailKey, defaults.Email),
            Bio = PlayerPrefs.GetString(BioKey, defaults.Bio)
        };
    }

    public static void Save(SettingsData data)
    {
        PlayerPrefs.SetFloat(MasterVolumeKey, data.MasterVolume);
        PlayerPrefs.SetFloat(MusicVolumeKey, data.MusicVolume);
        PlayerPrefs.SetFloat(SfxVolumeKey, data.SfxVolume);
        PlayerPrefs.SetInt(MutedKey, data.IsMuted ? 1 : 0);
        PlayerPrefs.SetInt(QualityKey, data.QualityIndex);
        PlayerPrefs.SetInt(ResolutionKey, data.ResolutionIndex);
        PlayerPrefs.SetInt(FullscreenKey, data.IsFullscreen ? 1 : 0);
        PlayerPrefs.SetString(UsernameKey, data.Username);
        PlayerPrefs.SetString(EmailKey, data.Email);
        PlayerPrefs.SetString(BioKey, data.Bio);
        PlayerPrefs.Save();
    }
}