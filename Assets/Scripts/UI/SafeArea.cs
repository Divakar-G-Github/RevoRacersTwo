using UnityEngine;

// Unity adds a RectTransform automatically and prevents removing it
[RequireComponent(typeof(RectTransform))]
public class SafeArea : MonoBehaviour
{
    private RectTransform _rectTransform;

    // Last values we applied - used to detect changes
    private Rect _lastSafeArea;
    private Vector2Int _lastScreenSize;
    private ScreenOrientation _lastOrientation;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        ApplySafeArea();
    }

    private void Update()
    {
        // Only do the work when something has changed
        bool changed = Screen.safeArea != _lastSafeArea
            || Screen.width != _lastScreenSize.x
            || Screen.height != _lastScreenSize.y
            || Screen.orientation != _lastOrientation;

        if (changed)
        {
            ApplySafeArea();
        }
    }

    private void ApplySafeArea()
    {
        Rect safeArea = Screen.safeArea;

        // Remember what we applied
        _lastSafeArea = safeArea;
        _lastScreenSize = new Vector2Int(Screen.width, Screen.height);
        _lastOrientation = Screen.orientation;

        if (Screen.width <= 0 || Screen.height <= 0)
            return;

        // Convert pixels to 0..1 anchors
        Vector2 anchorMin = safeArea.position;
        Vector2 anchorMax = safeArea.position + safeArea.size;

        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;

        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;

        // Apply Safe Area
        _rectTransform.anchorMin = anchorMin;
        _rectTransform.anchorMax = anchorMax;

        _rectTransform.offsetMin = Vector2.zero;
        _rectTransform.offsetMax = Vector2.zero;
    }
}