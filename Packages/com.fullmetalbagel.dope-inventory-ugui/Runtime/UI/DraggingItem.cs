using System;
using Unity.Mathematics;
using UnityEngine;

namespace DopeGrid.Inventory;

public sealed class DraggingItem
{
    // public InventoryItemInstanceId InstanceId { get; }
    public Guid ItemInstanceId { get; }
    public RectTransform View { get; }
    public RotationDegree Rotation { get; set; }

    public IUIInventory? TargetInventory { get; set; }
    public int2 TargetPosition { get; set; } = default;
    public int LastFrame { get; set; } = 0;

    public DraggingItem(Guid itemInstanceId, RectTransform view)
    {
        ItemInstanceId = itemInstanceId;
        View = view;
    }
}
