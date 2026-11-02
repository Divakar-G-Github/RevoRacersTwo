
using UnityEngine;
using UnityEngine.UI;

public class MapButtonView : MonoBehaviour
{
    [SerializeField] private Button _button;
    [SerializeField] private GameObject _lockOverlay;

    private int _mapIndex;

    public void Setup(int mapIndex, bool isUnlocked)
    {
        _mapIndex = mapIndex;

        _lockOverlay.SetActive(!isUnlocked);

        _button.interactable = isUnlocked;
    }

    private void OnEnable()
    {
        _button.onClick.AddListener(OnMapClicked);
    }

    private void OnDisable()
    {
        _button.onClick.RemoveListener(OnMapClicked);
    }

    private void OnMapClicked()
    {
        Debug.Log("Selected Map: " + (_mapIndex + 1));
    }
}
