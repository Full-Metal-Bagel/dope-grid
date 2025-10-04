using DopeGrid;
using DopeGrid.Standard;
using NUnit.Framework;

public class StandardGridBoardTests
{
    private GridBoard _gridBoard;

    [SetUp]
    public void Setup()
    {
        _gridBoard = new GridBoard(10, 10);
    }

    [TearDown]
    public void TearDown()
    {
        _gridBoard.Dispose();
    }

    [Test]
    public void Constructor_CreatesEmptyBoard()
    {
        Assert.AreEqual(10, _gridBoard.Width);
        Assert.AreEqual(10, _gridBoard.Height);
        Assert.AreEqual(0, _gridBoard.ItemCount);
        Assert.AreEqual(100, _gridBoard.FreeSpace);
    }

    [Test]
    public void Constructor_WithShape_ClonesShape()
    {
        // Create a 5x5 container with some cells marked as obstacles
        var containerShape = new GridShape(5, 5);
        containerShape[0, 0] = true; // Mark one cell as occupied/obstacle
        containerShape[4, 4] = true; // Mark another cell as occupied/obstacle

        var board = new GridBoard(containerShape);

        Assert.AreEqual(5, board.Width);
        Assert.AreEqual(5, board.Height);
        Assert.AreEqual(23, board.FreeSpace); // 25 - 2 occupied cells

        // Verify the occupied cells are preserved
        Assert.IsTrue(board.IsCellOccupied(new GridPosition(0, 0)));
        Assert.IsTrue(board.IsCellOccupied(new GridPosition(4, 4)));
        Assert.IsFalse(board.IsCellOccupied(new GridPosition(2, 2)));

        board.Dispose();
        containerShape.Dispose();
    }

    [Test]
    public void TryAddItem_AddsItemSuccessfully()
    {
        var itemShape = Shapes.ImmutableSquare(2);

        var (index, rotation) = _gridBoard.TryAddItem(itemShape);

        Assert.IsTrue(index >= 0);
        Assert.AreEqual(1, _gridBoard.ItemCount);
        Assert.AreEqual(96, _gridBoard.FreeSpace);
    }

    [Test]
    public void TryAddItemAt_AddsItemAtSpecificPosition()
    {
        var itemShape = Shapes.ImmutableSquare(2);

        var index = _gridBoard.TryAddItemAt(itemShape, new GridPosition(3, 4));

        Assert.IsTrue(index >= 0);
        Assert.AreEqual(1, _gridBoard.ItemCount);
        Assert.IsTrue(_gridBoard.IsCellOccupied(new GridPosition(3, 4)));
        Assert.IsTrue(_gridBoard.IsCellOccupied(new GridPosition(4, 4)));
        Assert.IsTrue(_gridBoard.IsCellOccupied(new GridPosition(3, 5)));
        Assert.IsTrue(_gridBoard.IsCellOccupied(new GridPosition(4, 5)));
    }

    [Test]
    public void TryAddItemAt_FailsWhenPositionOccupied()
    {
        var itemShape = Shapes.ImmutableSquare(2);

        _gridBoard.TryAddItemAt(itemShape, new GridPosition(3, 4));
        var secondAdd = _gridBoard.TryAddItemAt(itemShape, new GridPosition(3, 4));

        Assert.IsTrue(secondAdd < 0);
        Assert.AreEqual(1, _gridBoard.ItemCount);
    }

    [Test]
    public void TryAddItemAt_FailsWhenOutOfBounds()
    {
        var itemShape = Shapes.ImmutableSingle();

        var index = _gridBoard.TryAddItemAt(itemShape, new GridPosition(10, 10));

        Assert.IsTrue(index < 0);
        Assert.AreEqual(0, _gridBoard.ItemCount);
    }

    [Test]
    public void TryAddItemAt_FailsWhenPartiallyOverlapping()
    {
        var itemShape = Shapes.ImmutableSquare(3);

        _gridBoard.TryAddItemAt(Shapes.ImmutableSingle(), new GridPosition(2, 2));
        var index = _gridBoard.TryAddItemAt(itemShape, new GridPosition(1, 1));

        Assert.IsTrue(index < 0);
        Assert.AreEqual(1, _gridBoard.ItemCount);
    }

    [Test]
    public void RemoveItem_RemovesItemSuccessfully()
    {
        var itemShape = Shapes.ImmutableSquare(2);
        _gridBoard.TryAddItemAt(itemShape, new GridPosition(3, 4));

        _gridBoard.RemoveItem(0);

        Assert.AreEqual(0, _gridBoard.ItemCount);
        Assert.AreEqual(100, _gridBoard.FreeSpace);
        Assert.IsFalse(_gridBoard.IsCellOccupied(new GridPosition(3, 4)));
    }

    [Test]
    public void RemoveItem_MultipleItems_RemovesCorrectly()
    {
        var item1 = Shapes.ImmutableSquare(2);
        var item2 = Shapes.ImmutableSingle();

        _gridBoard.TryAddItemAt(item1, new GridPosition(0, 0));
        _gridBoard.TryAddItemAt(item2, new GridPosition(5, 5));

        _gridBoard.RemoveItem(0);

        Assert.AreEqual(1, _gridBoard.ItemCount);
        Assert.IsFalse(_gridBoard.IsCellOccupied(new GridPosition(0, 0)));
        Assert.IsTrue(_gridBoard.IsCellOccupied(new GridPosition(5, 5)));
    }

    [Test]
    public void Clear_RemovesAllItems()
    {
        _gridBoard.TryAddItem(Shapes.ImmutableSquare(2));
        _gridBoard.TryAddItem(Shapes.ImmutableSingle());
        _gridBoard.TryAddItem(Shapes.ImmutableLine(3));

        _gridBoard.Clear();

        Assert.AreEqual(0, _gridBoard.ItemCount);
        Assert.AreEqual(100, _gridBoard.FreeSpace);
    }

    [Test]
    public void Items_ReturnsAddedItems()
    {
        var item1 = Shapes.ImmutableSquare(2);
        var item2 = Shapes.ImmutableSingle();

        _gridBoard.TryAddItem(item1);
        _gridBoard.TryAddItem(item2);

        Assert.AreEqual(2, _gridBoard.ItemCount);
        Assert.AreEqual(item1, _gridBoard.GetItemShape(0));
        Assert.AreEqual(item2, _gridBoard.GetItemShape(1));
    }

    [Test]
    public void ItemPositions_ReturnsCorrectPositions()
    {
        var item1 = Shapes.ImmutableSquare(2);
        var item2 = Shapes.ImmutableSingle();

        _gridBoard.TryAddItemAt(item1, new GridPosition(1, 2));
        _gridBoard.TryAddItemAt(item2, new GridPosition(5, 6));

        Assert.AreEqual(2, _gridBoard.ItemCount);
        Assert.AreEqual(new GridPosition(1, 2), _gridBoard.GetItemPosition(0));
        Assert.AreEqual(new GridPosition(5, 6), _gridBoard.GetItemPosition(1));
    }

    [Test]
    public void TryAddItem_FindsFirstAvailablePosition()
    {
        var item = Shapes.ImmutableSingle();

        _gridBoard.TryAddItem(item);
        var pos = _gridBoard.GetItemPosition(0);

        Assert.AreEqual(GridPosition.Zero, pos); // Should place at top-left first
    }

    [Test]
    public void Clone_CreatesIndependentCopy()
    {
        _gridBoard.TryAddItem(Shapes.ImmutableSquare(2));

        var clone = _gridBoard.Clone();

        Assert.AreEqual(_gridBoard.Width, clone.Width);
        Assert.AreEqual(_gridBoard.Height, clone.Height);
        Assert.AreEqual(_gridBoard.ItemCount, clone.ItemCount);

        clone.TryAddItem(Shapes.ImmutableSingle());

        Assert.AreEqual(1, _gridBoard.ItemCount);
        Assert.AreEqual(2, clone.ItemCount);

        clone.Dispose();
    }

    [Test]
    public void IsCellOccupied_ReturnsFalseForEmptyCell()
    {
        Assert.IsFalse(_gridBoard.IsCellOccupied(new GridPosition(5, 5)));
    }

    [Test]
    public void IsCellOccupied_ReturnsTrueForOccupiedCell()
    {
        _gridBoard.TryAddItemAt(Shapes.ImmutableSingle(), new GridPosition(5, 5));

        Assert.IsTrue(_gridBoard.IsCellOccupied(new GridPosition(5, 5)));
    }

    [Test]
    public void FreeSpace_UpdatesCorrectly()
    {
        Assert.AreEqual(100, _gridBoard.FreeSpace);

        _gridBoard.TryAddItem(Shapes.ImmutableSquare(2)); // 4 cells
        Assert.AreEqual(96, _gridBoard.FreeSpace);

        _gridBoard.TryAddItem(Shapes.ImmutableLine(3)); // 3 cells
        Assert.AreEqual(93, _gridBoard.FreeSpace);

        _gridBoard.RemoveItem(0); // Remove 4 cells
        Assert.AreEqual(97, _gridBoard.FreeSpace);
    }
}
