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

    public DraggingItem(int instanceId, ItemDefinition definition, RectTransform view, RotationDegree rotation = RotationDegree.None)
    {
        InstanceId = instanceId;
        Definition = definition;
        View = view;
        Rotation = rotation;
    }

    public int2 GetGridPosition(InventoryView inventoryView)
    {
        return inventoryView.GetGridPosition(Shape.Bound, View.position);
    }
}
