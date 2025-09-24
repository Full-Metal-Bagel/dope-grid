using System;
using DopeGrid.Native;
using Unity.Mathematics;
using UnityEngine;

namespace DopeGrid.Inventory;

public class DraggingItem
{
    public int InstanceId { get; }
    public int InventoryId { get; }
    public ItemDefinition Definition { get; }
    public Guid DefinitionId => Definition.Id;
    public ImmutableGridShape Shape => Definition.Shape.GetRotatedShape(Rotation);
    public RectTransform View { get; }
    public RotationDegree Rotation { get; set; }

    public DraggingItem(int instanceId, int inventoryId, ItemDefinition definition, RectTransform view, RotationDegree rotation = RotationDegree.None)
    {
        InstanceId = instanceId;
        InventoryId = inventoryId;
        Definition = definition;
        View = view;
        Rotation = rotation;
    }

    public int2 GetGridPosition(InventoryView inventoryView)
    {
        var localPos = inventoryView.RectTransform.InverseTransformPoint(View.position);
        localPos.y = -localPos.y; // Convert to top-left origin

        var gridX = Mathf.RoundToInt(localPos.x / inventoryView.CellSize.x);
        var gridY = Mathf.RoundToInt(localPos.y / inventoryView.CellSize.y);

        return new int2(gridX, gridY);
    }
}
