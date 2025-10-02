using DopeGrid.Inventory;
using UnityEngine;

public class Items : MonoBehaviour
{
    private ulong _itemInstanceId = 0;
    public SharedInventoryData SharedInventoryData { get; } = new();

    public InventoryItemInstanceId NextItemInstanceId
    {
        get
        {
            return (InventoryItemInstanceId)_itemInstanceId++;
        }
    }
}
