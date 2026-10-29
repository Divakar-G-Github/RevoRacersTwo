using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmPopup : MonoBehaviour
{
    [SerializeField] private TMP_Text _messageText;
    [SerializeField] private Button _confirmButton;
    [SerializeField] private Button _cancelButton;
    [SerializeField] private CanvasGroup _blockedPanel;

    private Action _onConfirm;

    public void Show(string message, Action onConfirm)
    {
        _messageText.text = message;
        _onConfirm = onConfirm;
        SetPanelBlocked(true);
        gameObject.SetActive(true);
    }

    private void OnEnable()
    {
        _confirmButton.onClick.AddListener(OnConfirmClicked);
        _cancelButton.onClick.AddListener(OnCancelClicked);
    }

    private void OnDisable()
    {
        _confirmButton.onClick.RemoveListener(OnConfirmClicked);
        _cancelButton.onClick.RemoveListener(OnCancelClicked);
        SetPanelBlocked(false);
        _onConfirm = null;
    }

    private void OnConfirmClicked()
    {
        Action callback = _onConfirm; // keep it: Hide() clears _onConfirm
        Hide();
        callback?.Invoke();
    }

    private void OnCancelClicked() { Hide(); }
    private void Hide() { gameObject.SetActive(false); }

    private void SetPanelBlocked(bool isBlocked)
    {
        _blockedPanel.interactable = !isBlocked;
        _blockedPanel.blocksRaycasts = !isBlocked;
        _blockedPanel.alpha = isBlocked ? 0.6f : 1f;
    }
}