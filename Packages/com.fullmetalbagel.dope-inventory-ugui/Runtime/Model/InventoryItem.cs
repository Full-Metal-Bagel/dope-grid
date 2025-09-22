using System;
using DopeGrid.Native;
using Unity.Mathematics;

namespace DopeGrid.Inventory;

public readonly record struct InventoryItem(int InstanceId, ItemDefinition Definition, RotationDegree Rotation, int2 Position)
{
    public int InstanceId { get; } = InstanceId;
    public ItemDefinition Definition { get; } = Definition;
    public Guid DefinitionId => Definition.Id;
    public ImmutableGridShape Shape => Definition.Shape.GetRotatedShape(Rotation);
    public RotationDegree Rotation { get; } = Rotation;
    public int2 Position { get; } = Position;
}
