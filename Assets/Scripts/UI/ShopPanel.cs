using UnityEngine;
using UnityEngine.UI;

// Connects the shop UI to the car selection and the buying rules.
public class ShopPanel : MonoBehaviour
{
    [SerializeField] private UIManager _uiManager;

    [Header("Cars")]
    [SerializeField] private CarView[] _cars;   // Car1, Car2, Car3, Car4

    [Header("Buttons")]
    [SerializeField] private Button _leftButton;
    [SerializeField] private Button _rightButton;
    [SerializeField] private Button _applyButton;

    [Header("Locked car")]
    [SerializeField] private GameObject _unpurchasedGroup;   // UnPurchased (Lock, Buy, BuyError)
    [SerializeField] private Button _buyButton;
    [SerializeField] private GameObject _buyError;           // "Insufficient Coins"

    [Header("Effects")]
    [SerializeField] private CoinFlyEffect _coinFlyEffect;

    private ShopData _workingData;

    // ---------- Lifecycle ----------

    private void OnEnable()
    {
        _workingData = ShopStorage.Load();
        ClampSavedValues();
        ShowSelection();

        _leftButton.onClick.AddListener(OnLeftClicked);
        _rightButton.onClick.AddListener(OnRightClicked);
        _applyButton.onClick.AddListener(OnApplyClicked);
        _buyButton.onClick.AddListener(OnBuyClicked);

        foreach (CarView car in _cars)
        {
            car.ColorClicked += OnColorClicked;
        }
    }

    private void OnDisable()
    {
        _leftButton.onClick.RemoveListener(OnLeftClicked);
        _rightButton.onClick.RemoveListener(OnRightClicked);
        _applyButton.onClick.RemoveListener(OnApplyClicked);
        _buyButton.onClick.RemoveListener(OnBuyClicked);

        foreach (CarView car in _cars)
        {
            car.ColorClicked -= OnColorClicked;
        }
    }

    // ---------- Button handlers ----------

    private void OnLeftClicked()
    {
        _workingData.SelectedCarIndex =
            (_workingData.SelectedCarIndex - 1 + _cars.Length) % _cars.Length;
        ShowSelection();
    }

    private void OnRightClicked()
    {
        _workingData.SelectedCarIndex =
            (_workingData.SelectedCarIndex + 1) % _cars.Length;
        ShowSelection();
    }

    private void OnColorClicked(int colorIndex)
    {
        _workingData.SelectedColorIndex = colorIndex;
        ShowSelection();
    }

    private void OnBuyClicked()
    {
        int carIndex = _workingData.SelectedCarIndex;
        if (IsOwned(carIndex))
        {
            return;
        }

        int price = _cars[carIndex].Price;

        if (_uiManager.Coins < price)
        {
            _buyError.SetActive(true);   // not enough coins
            return;
        }

        _coinFlyEffect.Play((RectTransform)_buyButton.transform);   // visual only

        _uiManager.SetCoins(_uiManager.Coins - price);
        _workingData.MarkPurchased(carIndex);
        ShopStorage.SavePurchases(_workingData);
        UpdatePurchaseUI();
    }

    private void OnApplyClicked()
    {
        if (!IsOwned(_workingData.SelectedCarIndex))
        {
            return;   // safety: a locked car can never be saved as selected
        }

        ShopStorage.Save(_workingData);
        _uiManager.Back();
    }

    // ---------- Showing the current choice ----------

    private void ShowSelection()
    {
        for (int i = 0; i < _cars.Length; i++)
        {
            _cars[i].gameObject.SetActive(i == _workingData.SelectedCarIndex);
        }

        _cars[_workingData.SelectedCarIndex].ShowColor(_workingData.SelectedColorIndex);
        UpdatePurchaseUI();
    }

    // Lock group, error text and Apply depend on whether the car is owned.
    private void UpdatePurchaseUI()
    {
        bool isOwned = IsOwned(_workingData.SelectedCarIndex);

        _unpurchasedGroup.SetActive(!isOwned);
        _buyError.SetActive(false);
        _applyButton.interactable = isOwned;
    }

    // ---------- Helpers ----------

    // A car is owned if it is free or was bought.
    private bool IsOwned(int carIndex)
    {
        return _cars[carIndex].Price <= 0 || _workingData.IsPurchased(carIndex);
    }

    private void ClampSavedValues()
    {
        if (_workingData.SelectedCarIndex < 0 || _workingData.SelectedCarIndex >= _cars.Length)
        {
            _workingData.SelectedCarIndex = 0;
        }

        // the saved car may be locked again after a reset: fall back to the free car
        if (!IsOwned(_workingData.SelectedCarIndex))
        {
            _workingData.SelectedCarIndex = 0;
        }

        int colorCount = _cars[_workingData.SelectedCarIndex].ColorCount;
        if (_workingData.SelectedColorIndex < 0 || _workingData.SelectedColorIndex >= colorCount)
        {
            _workingData.SelectedColorIndex = 0;
        }
    }

    // Right-click the ShopPanel component header to use this for demos.
    [ContextMenu("Reset Purchased Cars (demo)")]
    private void ResetPurchases()
    {
        ShopStorage.ClearPurchases();

        if (!Application.isPlaying || _workingData == null)
        {
            return;
        }

        _workingData.PurchasedCars.Clear();
        if (!IsOwned(_workingData.SelectedCarIndex))
        {
            _workingData.SelectedCarIndex = 0;
        }
        ShowSelection();
    }
}