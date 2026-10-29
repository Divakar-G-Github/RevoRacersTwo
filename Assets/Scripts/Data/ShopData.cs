using System.Collections.Generic;

// Plain C# class: only holds the shop choices. No UI, no saving.
public class ShopData
{
    public int SelectedCarIndex { get; set; } = 0;
    public int SelectedColorIndex { get; set; } = 0;

    // Indexes of the cars the player has bought (a HashSet is a Python set).
    public HashSet<int> PurchasedCars { get; } = new HashSet<int>();

    public bool IsPurchased(int carIndex)
    {
        return PurchasedCars.Contains(carIndex);
    }

    public void MarkPurchased(int carIndex)
    {
        PurchasedCars.Add(carIndex);
    }
}