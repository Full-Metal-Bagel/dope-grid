using System;
using DopeGrid.Native;
using Unity.Mathematics;
using UnityEngine;

namespace DopeGrid.Inventory;

public class DraggingItem
{
    public int InstanceId { get; }
    public ItemDefinition Definition { get; }
    public Guid DefinitionId => Definition.Id;
    public ImmutableGridShape Shape => Definition.Shape.GetRotatedShape(Rotation);
    public RectTransform View { get; }
    public RotationDegree Rotation { get; set; }

    public Inventory SourceInventory { get; set; } = default;
    public Inventory TargetInventory { get; set; } = default;
    public int2 TargetPosition { get; set; } = default;
    public int LastFrame { get; set; } = 0;

    public DraggingItem(int instanceId, ItemDefinition definition, RectTransform view, RotationDegree rotation = RotationDegree.None)
    {
        InstanceId = instanceId;
        Definition = definition;
        View = view;
        Rotation = rotation;
    }

    public int2 GetGridPosition(RectTransform transform, float2 cellSize)
    {
        return transform.GetGridPosition(cellSize, Shape.Bound, View.position);
    }
}
