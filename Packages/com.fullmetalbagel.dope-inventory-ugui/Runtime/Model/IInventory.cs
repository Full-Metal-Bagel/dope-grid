namespace DopeGrid.Inventory;

public interface IInventory : IReadOnlyIndexedGridBoard
{
    IndexedGridBoard.Enumerator GetEnumerator();
    (BoardItemData from, BoardItemData to) TryMoveItem(IInventory from, int itemId, ImmutableGridShape shape, int x, int y);
    bool CanMoveItem(IInventory from, int itemId, ImmutableGridShape shape, int x, int y);
    BoardItemData RemoveItem(int id);
    BoardItemData TryAddItemAt(ImmutableGridShape shape, int x, int y);
}

public sealed class NaiveInventory : IInventory
{
    private readonly IndexedGridBoard _board;

    public NaiveInventory(IndexedGridBoard board) => _board = board;
    public BoardItemData GetItemById(int id) => _board.GetItemById(id);
    public int Width => _board.Width;
    public int Height => _board.Height;
    public bool IsOccupied(int x, int y) => _board.IsOccupied(x, y);
    public int this[int x, int y] => _board[x, y];
    public IndexedGridBoard.Enumerator GetEnumerator() => _board.GetEnumerator();
    public BoardItemData RemoveItem(int id) => _board.RemoveItem(id);
    public BoardItemData TryAddItemAt(ImmutableGridShape shape, int x, int y) => _board.TryAddItemAt(shape, x, y);

    public (BoardItemData from, BoardItemData to) TryMoveItem(IInventory from, int itemId, ImmutableGridShape shape, int x, int y)
    {
        var isSameInventory = ReferenceEquals(this, from);
        var item = from.GetItemById(itemId);
        var canMove = CanMoveItem(from, itemId, shape, x, y);
        if (item.IsInvalid || !canMove) return (item, BoardItemData.Invalid);

        if (isSameInventory)
        {
            _board.RemoveItem(itemId);
            var newItem = _board.TryAddItemAt(shape, x, y);
            return (item, newItem);
        }
        else
        {
            var newItem = _board.TryAddItemAt(shape, x, y);
            from.RemoveItem(itemId);
            return (item, newItem);
        }
    }

    public bool CanMoveItem(IInventory from, int itemId, ImmutableGridShape shape, int x, int y)
    {
        var isSameInventory = ReferenceEquals(this, from);
        if (isSameInventory)
        {
            return this.CanMoveItem(itemId, shape, x, y);
        }
        return this.CanPlaceItem(shape, x, y, default(int));
    }
}
