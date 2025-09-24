using System;
using DopeGrid.Native;
using UnityEngine;

namespace DopeGrid.Inventory;

public readonly struct MoveItemCommand
{
    public int InstanceId { get; }
    public ItemDefinition Definition { get; }
    public RotationDegree Rotation { get; }
    public Vector2 WorldPosition { get; }
    public Guid DefinitionId => Definition.Id;
    public ImmutableGridShape Shape => Definition.Shape.GetRotatedShape(Rotation);

    public MoveItemCommand(int instanceId, ItemDefinition definition, RotationDegree rotation, Vector2 worldPosition)
    {
        InstanceId = instanceId;
        Definition = definition;
        Rotation = rotation;
        WorldPosition = worldPosition;
    }
}
