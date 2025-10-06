namespace DopeGrid.Inventory;

public interface IInventory : IReadOnlyIndexedGridBoard
{
    IndexedGridBoard.Enumerator GetEnumerator();
}

public sealed class Inventory : IInventory
{
    private readonly IndexedGridBoard _board;
    public Inventory(IndexedGridBoard board) => _board = board;
    public BoardItemData GetItemById(int id) => _board.GetItemById(id);
    public int Width => _board.Width;
    public int Height => _board.Height;
    public bool IsOccupied(int x, int y) => _board.IsOccupied(x, y);
    public int this[int x, int y] => _board[x, y];
    public IndexedGridBoard.Enumerator GetEnumerator() => _board.GetEnumerator();
}
