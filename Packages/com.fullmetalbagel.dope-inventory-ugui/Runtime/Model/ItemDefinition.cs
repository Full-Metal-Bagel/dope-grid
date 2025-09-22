using System;
using DopeGrid.Native;

namespace DopeGrid.Inventory;

public readonly record struct ItemDefinition(Guid Id, ImmutableGridShape Shape)
{
    public Guid Id { get; } = Id;
    public ImmutableGridShape Shape { get; } = Shape;
}
