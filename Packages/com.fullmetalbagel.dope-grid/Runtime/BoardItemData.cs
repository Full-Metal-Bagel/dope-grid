namespace DopeGrid;

public readonly record struct BoardItemData(int Id, ImmutableGridShape Shape, int X, int Y)
{
    public int Id { get; } = Id;
    public ImmutableGridShape Shape { get; } = Shape;
    public int X { get; } = X;
    public int Y { get; } = Y;

    public static BoardItemData Invalid => new(-1, ImmutableGridShape.Empty, -1, -1);
    public bool IsInvalid => Id < 0 || X < 0 || Y < 0 || Shape.Id < 0 || Shape == ImmutableGridShape.Empty;
    public bool IsValid => !IsInvalid;
}
