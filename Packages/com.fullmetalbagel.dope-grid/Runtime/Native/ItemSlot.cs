namespace DopeGrid.Native;

internal readonly record struct ItemSlot(ImmutableGridShape Shape, GridPosition Position)
{
    public static readonly ItemSlot Invalid = new(ImmutableGridShape.Empty, GridPosition.Invalid);
    public ImmutableGridShape Shape { get; } = Shape;
    public GridPosition Position { get; } = Position;
    public bool IsValid => Position.IsValid || !Shape.IsEmpty;
}
