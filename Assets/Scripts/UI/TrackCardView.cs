
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TrackCardView : MonoBehaviour
{
    [Header("Card References")]
    [SerializeField] private Button _button;
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private Image _previewImage;
    [SerializeField] private GameObject _lockOverlay;
    [SerializeField] private TMP_Text _bestTimeText;
    [SerializeField] private GameObject _selectedMark;

    private int _trackIndex;
    private Action<int> _onClicked;

    private void Awake()
    {
        _button.onClick.AddListener(OnButtonClicked);
    }

    private void OnDestroy()
    {
        _button.onClick.RemoveListener(OnButtonClicked);
    }

    public void Setup(
        int trackIndex,
        TrackDefinition track,
        bool isUnlocked,
        float bestTime,
        Action<int> onClicked)
    {
        _trackIndex = trackIndex;
        _onClicked = onClicked;

        _nameText.text = string.IsNullOrEmpty(track.DisplayName)
            ? $"Map {trackIndex + 1}"
            : track.DisplayName;

        if (_previewImage != null && track.Preview != null)
        {
            _previewImage.sprite = track.Preview;
        }

        // Only the lock overlay is hidden for unlocked maps.
        _lockOverlay.SetActive(!isUnlocked);

        // Locked maps cannot be selected.
        _button.interactable = isUnlocked;

        if (_bestTimeText != null)
        {
            _bestTimeText.gameObject.SetActive(isUnlocked);

            _bestTimeText.text = bestTime > 0f
                ? $"BEST {TimeFormatter.Format(bestTime)}"
                : "NO TIME YET";
        }

        SetSelected(false);
    }

    public void SetSelected(bool isSelected)
    {
        if (_selectedMark != null)
        {
            _selectedMark.SetActive(isSelected);
        }
    }

    private void OnButtonClicked()
    {
        _onClicked?.Invoke(_trackIndex);
    }
}
