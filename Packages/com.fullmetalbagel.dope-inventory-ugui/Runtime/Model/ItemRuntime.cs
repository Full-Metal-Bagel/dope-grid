using DopeGrid.Native;
using Unity.Mathematics;

namespace DopeGrid.Inventory;

public readonly record struct ItemRuntime(int InstanceId, ItemDefinition Definition, RotationDegree Rotation, int2 Position)
{
    public int InstanceId { get; } = InstanceId;
    public ItemDefinition Definition { get; } = Definition;
    public int DefinitionId => Definition.Id;
    public ImmutableGridShape Shape => Definition.Shape.GetRotatedShape(Rotation);
    public RotationDegree Rotation { get; } = Rotation;
    public int2 Position { get; } = Position;
}
