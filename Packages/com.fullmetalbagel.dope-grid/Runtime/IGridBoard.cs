namespace DopeGrid;

public interface IGridBoard<out T> : IReadOnlyGridShape<T>
{
    (BoardItemData item, RotationDegree rotation) TryAddItem(ImmutableGridShape item);
    (BoardItemData item, RotationDegree rotation) TryAddItemAt(ImmutableGridShape shape, int x, int y);
    BoardItemData RemoveItem(int id);
    void Reset();
}

public interface IReadOnlyGridBoard<out T> : IReadOnlyGridShape<T>
{
    BoardItemData GetItemById(int id);
}

public interface IIndexedGridBoard : IGridBoard<int> { }
public interface IReadOnlyIndexedGridBoard : IReadOnlyGridBoard<int> { }

public static class GridBoardExtension
{
    public static int GetItemId<T>(this T board, int x, int y)
        where T : IReadOnlyIndexedGridBoard
    {
        return board[x, y];
    }

    public static BoardItemData GetItemOnPosition<T>(this T board, int x, int y)
        where T : IReadOnlyIndexedGridBoard
    {
        return board.GetItemById(board.GetItemId(x, y));
    }
}
