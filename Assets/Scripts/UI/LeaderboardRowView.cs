using System.Globalization;
using TMPro;
using UnityEngine;

// Goes on every player row (Player1, Player2, ...). Writes one record into the row's texts.
public class LeaderboardRowView : MonoBehaviour
{
    [SerializeField] private TMP_Text _rankText;   // only Player4 and later have one (leave empty on 1-3)
    [SerializeField] private TMP_Text _nameText;   // PlayerNameText
    [SerializeField] private TMP_Text _timeText;   // BestTimeText

    public void Bind(int rank, LeaderboardRecord record)
    {
        if (_rankText != null)
        {
            _rankText.text = rank.ToString();
        }

        _nameText.text = record.PlayerName;
        _timeText.text = FormatTime(record.TimeSeconds);
    }

    // 83.456 seconds -> "01:23.456"
    private static string FormatTime(float seconds)
    {
        int minutes = Mathf.FloorToInt(seconds / 60f);
        float rest = seconds - minutes * 60f;
        return $"{minutes:00}:{rest.ToString("00.000", CultureInfo.InvariantCulture)}";
    }
}