using System;
using Unity.Mathematics;
using UnityEngine;

namespace DopeGrid.Inventory;

public class DraggingItem
{
    // public InventoryItemInstanceId InstanceId { get; }
    public UIImageGridDefinitionData UIDefinition { get; }
    public Guid DefinitionId => UIDefinition.Id;
    public ImmutableGridShape DefinitionShape => UIDefinition.Shape;
    public Sprite Image => UIDefinition.Image;
    public ImmutableGridShape Shape => DefinitionShape.GetRotatedShape(Rotation);
    public RectTransform View { get; }
    public RotationDegree Rotation { get; set; }

    // public Inventory SourceInventory { get; set; } = default;
    // public Inventory TargetInventory { get; set; } = default;
    public int2 TargetPosition { get; set; } = default;
    public int LastFrame { get; set; } = 0;

    public DraggingItem(RectTransform view, UIImageGridDefinitionData definition, RotationDegree rotation = RotationDegree.None)
    {
        // InstanceId = id;
        UIDefinition = definition;
        View = view;
        Rotation = rotation;
    }
    //
    // public int2 GetGridPosition(RectTransform transform, float2 cellSize)
    // {
    //     var (width, height) = Shape.Bound;
    //     return transform.GetGridPosition(cellSize, width, height, View.position);
    // }
}
