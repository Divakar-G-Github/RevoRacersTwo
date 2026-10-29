using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [System.Serializable]
    public class ScreenEntry
    {
        public ScreenType Type;
        public GameObject Root;
    }

    [Header("Screens")]
    [SerializeField] private List<ScreenEntry> _screens = new List<ScreenEntry>();
    [SerializeField] private ScreenType _startScreen = ScreenType.MainMenu;

    [Header("Coins")]
    [SerializeField] private TMP_Text _coinsText;
    [SerializeField] private int _startCoins = 1250;

    private readonly Dictionary<ScreenType, GameObject> _screenLookup = new Dictionary<ScreenType, GameObject>();
    private readonly Stack<ScreenType> _history = new Stack<ScreenType>();
    private ScreenType _currentScreen;

    public int Coins { get; private set; }

    private void Awake()
    {
        foreach (ScreenEntry entry in _screens)
        {
            if (entry.Root == null)
            {
                Debug.LogWarning($"UIManager: no root assigned for {entry.Type}.");
                continue;
            }
            _screenLookup[entry.Type] = entry.Root;
            entry.Root.SetActive(false);
        }

        _currentScreen = _startScreen;
        SetScreenActive(_currentScreen, true);
        SetCoins(_startCoins);
    }

    public void Show(ScreenType screen)
    {
        if (screen == _currentScreen) return;

        SetScreenActive(_currentScreen, false);
        _history.Push(_currentScreen);
        _currentScreen = screen;
        SetScreenActive(_currentScreen, true);
    }

    public void Back()
    {
        if (_history.Count == 0) return;

        SetScreenActive(_currentScreen, false);
        _currentScreen = _history.Pop();
        SetScreenActive(_currentScreen, true);
    }

    public void SetCoins(int amount)
    {
        Coins = Mathf.Max(0, amount);
        if (_coinsText != null)
        {
            _coinsText.text = Coins.ToString("N0");
        }
    }

    [ContextMenu("Add 100 Coins")]
    private void AddTestCoins()
    {
        SetCoins(Coins + 100);
    }

    private void SetScreenActive(ScreenType screen, bool isActive)
    {
        if (_screenLookup.TryGetValue(screen, out GameObject root))
        {
            root.SetActive(isActive);
        }
        else
        {
            Debug.LogWarning($"UIManager: screen {screen} is not registered.");
        }
    }
}