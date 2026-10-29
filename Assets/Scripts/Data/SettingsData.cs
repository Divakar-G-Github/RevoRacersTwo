using UnityEngine;

public class SettingsData
{
    // Audio (0 - 100)
    public float MasterVolume { get; set; } = 80f;
    public float MusicVolume { get; set; } = 70f;
    public float SfxVolume { get; set; } = 90f;
    public bool IsMuted { get; set; } = false;

    // Graphics
    public int QualityIndex { get; set; } = 0;
    public int ResolutionIndex { get; set; } = 2; // 1920 x 1080
    public bool IsFullscreen { get; set; } = true;

    // Account (the password is never saved)
    public string Username { get; set; } = "";
    public string Email { get; set; } = "";
    public string Bio { get; set; } = "";

    public static SettingsData CreateDefault()
    {
        SettingsData data = new SettingsData();
        data.QualityIndex = QualitySettings.names.Length - 1;
        return data;
    }
}