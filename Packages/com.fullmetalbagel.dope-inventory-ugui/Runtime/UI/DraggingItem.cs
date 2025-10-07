using System;
using UnityEngine;

namespace DopeGrid.Inventory;

public sealed class DraggingItem
{
    // public InventoryItemInstanceId InstanceId { get; }
    public Guid ItemInstanceId { get; }
    public RectTransform View { get; }
    public RotationDegree Rotation { get; set; }

    public IUIInventory? TargetInventory { get; set; }
    public Vector2Int TargetPosition { get; set; } = default;
    public int LastFrame { get; set; } = 0;

    public DraggingItem(Guid itemInstanceId, RectTransform view, RotationDegree rotation)
    {
        ItemInstanceId = itemInstanceId;
        View = view;
        Rotation = rotation;
    }
}
