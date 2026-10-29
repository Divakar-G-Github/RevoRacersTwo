using UnityEngine;
using UnityEngine.UI;

// Starts the credits at the top every time the screen opens.
public class CreditsPanel : MonoBehaviour
{
    [SerializeField] private ScrollRect _scrollRect;

    private void OnEnable()
    {
        _scrollRect.StopMovement();                          // cancel any leftover swipe
        _scrollRect.content.anchoredPosition = Vector2.zero; // Content's top is at the top
    }
}