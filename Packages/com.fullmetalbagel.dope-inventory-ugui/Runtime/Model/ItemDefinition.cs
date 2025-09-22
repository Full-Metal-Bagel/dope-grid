using DopeGrid.Native;

namespace DopeGrid.Inventory;

public readonly record struct ItemDefinition(int Id, ImmutableGridShape Shape)
{
    public int Id { get; } = Id;
    public ImmutableGridShape Shape { get; } = Shape;
}
