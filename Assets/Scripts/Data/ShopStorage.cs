using UnityEngine;

// Only saves and loads ShopData.
public static class ShopStorage
{
    private const string CarKey = "Shop.SelectedCar";
    private const string ColorKey = "Shop.SelectedColor";
    private const string PurchasedKey = "Shop.PurchasedCars";

    public static ShopData Load()
    {
        ShopData data = new ShopData
        {
            SelectedCarIndex = PlayerPrefs.GetInt(CarKey, 0),
            SelectedColorIndex = PlayerPrefs.GetInt(ColorKey, 0)
        };

        // Purchased cars are stored as text, for example "1,3".
        string saved = PlayerPrefs.GetString(PurchasedKey, "");
        foreach (string part in saved.Split(','))
        {
            if (int.TryParse(part, out int carIndex))
            {
                data.PurchasedCars.Add(carIndex);
            }
        }

        return data;
    }

    // Saves everything (used by Apply).
    public static void Save(ShopData data)
    {
        PlayerPrefs.SetInt(CarKey, data.SelectedCarIndex);
        PlayerPrefs.SetInt(ColorKey, data.SelectedColorIndex);
        WritePurchases(data);
        PlayerPrefs.Save();
    }

    // Saves only the purchases (used right after buying a car).
    public static void SavePurchases(ShopData data)
    {
        WritePurchases(data);
        PlayerPrefs.Save();
    }

    // Used by the demo reset.
    public static void ClearPurchases()
    {
        PlayerPrefs.DeleteKey(PurchasedKey);
        PlayerPrefs.Save();
    }

    private static void WritePurchases(ShopData data)
    {
        PlayerPrefs.SetString(PurchasedKey, string.Join(",", data.PurchasedCars));
    }
}