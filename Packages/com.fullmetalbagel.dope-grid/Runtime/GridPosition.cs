namespace DopeGrid;

public readonly record struct GridPosition(int X, int Y)
{
    public static readonly GridPosition Zero = new(0, 0);
    public int X { get; } = X;
    public int Y { get; } = Y;

    public static implicit operator (int x, int y)(GridPosition pos) => (pos.X, pos.Y);
    public static implicit operator GridPosition((int x, int y) tuple) => new(tuple.x, tuple.y);

#if UNITY_MATHEMATICS
    public static implicit operator Unity.Mathematics.int2(GridPosition pos) => new(pos.X, pos.Y);
    public static implicit operator GridPosition(Unity.Mathematics.int2 value) => new(value.x, value.y);
#endif

    public void Deconstruct(out int x, out int y)
    {
        x = X;
        y = Y;
    }

    public override string ToString() => $"({X}, {Y})";
}
