using System.Collections.Generic;
using DopeGrid.Inventory;
using UnityEngine;

public class Items : MonoBehaviour
{
    private int _itemInstanceId = -1;
    public List<DraggingItem> DraggingItems { get; } = new();

    public int NextItemInstanceId
    {
        get
        {
            _itemInstanceId++;
            return _itemInstanceId;
        }
    }
}
