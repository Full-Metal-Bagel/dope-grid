namespace DopeGrid;

public interface IReadOnlyGridShape
{
    int Width { get; }
    int Height { get; }
    bool this[GridPosition pos] { get; }
    bool this[int x, int y] { get; }
    bool this[int index] { get; }
}
