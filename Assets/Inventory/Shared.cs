using DopeGrid.Inventory;
using UnityEngine;

public class Shared : MonoBehaviour
{
    public SharedInventoryData SharedData { get; } = new();
    public SharedUIInventoryData SharedUIData { get; } = new();
}
