namespace DopeGrid;

public interface IGridBoard<out T> : IReadOnlyGridShape<T>
{
    BoardItemData TryAddItem(ImmutableGridShape item);
    BoardItemData TryAddItemAt(ImmutableGridShape shape, int x, int y);
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

    public static bool CanMoveItem<T>(this T board, int itemId, ImmutableGridShape shape, int x, int y)
        where T : IReadOnlyIndexedGridBoard
    {
        if (!board.IsWithinBounds(shape, x, y, default(int)))
            return false;
        return board.CheckShapeCells(shape, x, y, (itemId, board), static (x, y, value, t) => !t.board.IsOccupied(x, y) || value == t.itemId, default(int));
    }
}
