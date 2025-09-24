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
        // Convert from Canvas coordinates (center pivot) to InventoryView coordinates (top-left pivot)
        var inventoryRect = inventoryView.RectTransform;
        var localPosInInventory = inventoryRect.InverseTransformPoint(View.position);

        // Account for the dragging ghost having center pivot - convert to top-left corner
        var itemSize = View.sizeDelta;
        var topLeftPos = localPosInInventory - new Vector3(itemSize.x * 0.5f, -itemSize.y * 0.5f, 0f);

        // Convert to top-left origin from the inventory view's pivot
        var rect = inventoryRect.rect;
        var size = rect.size;
        var left = -inventoryRect.pivot.x * size.x;
        var top = (1 - inventoryRect.pivot.y) * size.y;
        var fromTopLeft = new Vector2(topLeftPos.x - left, top - topLeftPos.y);

        var gridX = Mathf.RoundToInt(fromTopLeft.x / inventoryView.CellSize.x);
        var gridY = Mathf.RoundToInt(fromTopLeft.y / inventoryView.CellSize.y);

        return new int2(gridX, gridY);
    }

}
