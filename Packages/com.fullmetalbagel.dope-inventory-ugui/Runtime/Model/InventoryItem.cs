using System;
using DopeGrid.Native;
using Unity.Mathematics;

namespace DopeGrid.Inventory;

public readonly record struct InventoryItemInstanceId(ulong Id)
{
    public ulong Id { get; } = Id;
    public static implicit operator ulong(InventoryItemInstanceId instanceId) => instanceId.Id;
    public static explicit operator InventoryItemInstanceId(ulong id) => new(id);
}

public readonly record struct InventoryItem(int InstanceId, ItemDefinition Definition, RotationDegree Rotation, int2 Position)
{
    public static InventoryItem Invalid => new(-1, default, RotationDegree.None, default);

    public bool IsValid => InstanceId != -1;
    public bool IsInvalid => !IsValid;

    public int InstanceId { get; } = InstanceId;
    public ItemDefinition Definition { get; } = Definition;
    public Guid DefinitionId => Definition.Id;
    public ImmutableGridShape Shape => Definition.Shape.GetRotatedShape(Rotation);
    public RotationDegree Rotation { get; } = Rotation;
    public int2 Position { get; } = Position;
}
