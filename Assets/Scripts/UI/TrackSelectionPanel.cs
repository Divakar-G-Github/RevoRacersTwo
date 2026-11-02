
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TrackSelectionPanel : MonoBehaviour
{
    [Header("Maps")]
    [SerializeField] private TrackList _tracks;
    [SerializeField] private TrackCardView[] _existingCards;
    [SerializeField] private TrackCardView _cardPrefab;
    [SerializeField] private RectTransform _content;

    private readonly List<TrackCardView> _extraCards = new List<TrackCardView>();

    private int _selectedIndex;

    private void OnEnable()
    {
        RefreshCards();
        SelectTrack(GetValidSelection());
    }

    private void RefreshCards()
    {
        for (int i = 0; i < _tracks.Count; i++)
        {
            TrackCardView card = GetCard(i);

            card.gameObject.SetActive(true);
            card.gameObject.name = $"Map{i + 1}";

            card.Setup(
                i,
                _tracks.GetTrack(i),
                TrackProgress.IsUnlocked(i),
                TrackProgress.GetBestTime(i),
                OnCardClicked
            );
        }

        HideUnusedCards(_tracks.Count);

        LayoutRebuilder.ForceRebuildLayoutImmediate(_content);
    }

    private TrackCardView GetCard(int index)
    {
        if (index < _existingCards.Length)
        {
            return _existingCards[index];
        }

        int extraIndex = index - _existingCards.Length;

        while (_extraCards.Count <= extraIndex)
        {
            TrackCardView newCard = Instantiate(_cardPrefab, _content);
            _extraCards.Add(newCard);
        }

        return _extraCards[extraIndex];
    }

    private void HideUnusedCards(int usedCount)
    {
        for (int i = usedCount; i < _existingCards.Length; i++)
        {
            _existingCards[i].gameObject.SetActive(false);
        }

        int usedExtra = Mathf.Max(0, usedCount - _existingCards.Length);

        for (int i = usedExtra; i < _extraCards.Count; i++)
        {
            _extraCards[i].gameObject.SetActive(false);
        }
    }

    private int GetValidSelection()
    {
        int index = RaceSession.TrackIndex;

        bool valid =
            index >= 0 &&
            index < _tracks.Count &&
            TrackProgress.IsUnlocked(index);

        return valid ? index : 0;
    }

    private void SelectTrack(int index)
    {
        _selectedIndex = index;
        RaceSession.TrackIndex = index;

        for (int i = 0; i < _tracks.Count; i++)
        {
            GetCard(i).SetSelected(i == index);
        }
    }

    private void OnCardClicked(int index)
    {
        SelectTrack(index);
    }

    public string GetSelectedSceneName()
    {
        TrackDefinition track = _tracks.GetTrack(_selectedIndex);

        if (RaceSession.Mode == GameMode.TimeTrial)
        {
            return track.TimeTrialSceneName;
        }

        if (RaceSession.Mode == GameMode.HotLap)
        {
            return track.HotLapSceneName;
        }

        Debug.LogWarning("This game mode is not available yet.");
        return null;
    }
}
