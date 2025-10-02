using System;
using DopeGrid.Native;
using Unity.Mathematics;

namespace DopeGrid.Inventory;

public readonly record struct InventoryItemInstanceId(ulong Id)
{
    public static readonly InventoryItemInstanceId Invalid = new(ulong.MaxValue);

    public ulong Id { get; } = Id;
    public static implicit operator ulong(InventoryItemInstanceId instanceId) => instanceId.Id;
    public static explicit operator InventoryItemInstanceId(ulong id) => new(id);
}

public readonly record struct InventoryItem(InventoryItemInstanceId InstanceId, ItemDefinition Definition, RotationDegree Rotation, int2 Position)
{
    public static InventoryItem Invalid => new(InventoryItemInstanceId.Invalid, default, RotationDegree.None, default);

    public bool IsValid => InstanceId != InventoryItemInstanceId.Invalid;
    public bool IsInvalid => !IsValid;

    public InventoryItemInstanceId InstanceId { get; } = InstanceId;
    public ItemDefinition Definition { get; } = Definition;
    public Guid DefinitionId => Definition.Id;
    public ImmutableGridShape Shape => Definition.Shape.GetRotatedShape(Rotation);
    public RotationDegree Rotation { get; } = Rotation;
    public int2 Position { get; } = Position;
}
