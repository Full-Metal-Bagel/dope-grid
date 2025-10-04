using DopeGrid;
using DopeGrid.Native;
using NUnit.Framework;
using Unity.Collections;
using Unity.Mathematics;

public class GridBoardTests
{
    private GridBoard _gridBoard;

    [SetUp]
    public void Setup()
    {
        // GridBoard with empty grid (all cells = false)
        // In the new API, occupied cells are TRUE, empty cells are FALSE
        _gridBoard = new GridBoard(10, 10, Allocator.Temp);
    }

    [TearDown]
    public void TearDown()
    {
        _gridBoard.Dispose();
    }

    #region Helper Methods

    private GridShape CreateSquareShape(int size, Allocator allocator)
    {
        var shape = new GridShape(size, size, allocator);
        for (var y = 0; y < size; y++)
        for (var x = 0; x < size; x++)
            shape[x, y] = true;
        return shape;
    }

    #endregion

    #region Basic Inventory Tests

    [Test]
    public void Constructor_CreatesEmptyInventory()
    {
        Assert.AreEqual(10, _gridBoard.Width);
        Assert.AreEqual(10, _gridBoard.Height);
        Assert.AreEqual(0, _gridBoard.ItemCount);
        Assert.AreEqual(100, _gridBoard.FreeSpace);
    }

    [Test]
    public void TryAddItem_AddsItemSuccessfully()
    {
        var itemShape = Shapes.ImmutableSquare(2);

        // TryAddItem with free rotation is not implemented, use TryAddItemAt instead
        var index = _gridBoard.TryAddItemAt(itemShape, GridPosition.Zero);

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
    public void RemoveItem_RemovesItemSuccessfully()
    {
        var itemShape = Shapes.ImmutableSquare(2);

        _gridBoard.TryAddItemAt(itemShape, new GridPosition(2, 2));
        _gridBoard.RemoveItem(0);

        Assert.AreEqual(0, _gridBoard.ItemCount);
        Assert.AreEqual(100, _gridBoard.FreeSpace);
        Assert.IsFalse(_gridBoard.IsCellOccupied(new GridPosition(2, 2)));
        Assert.IsFalse(_gridBoard.IsCellOccupied(new GridPosition(3, 2)));
        Assert.IsFalse(_gridBoard.IsCellOccupied(new GridPosition(2, 3)));
        Assert.IsFalse(_gridBoard.IsCellOccupied(new GridPosition(3, 3)));
    }

    [Test]
    public void RemoveItem_HandlesInvalidIndex()
    {
        _gridBoard.RemoveItem(-1);
        _gridBoard.RemoveItem(10);

        Assert.AreEqual(0, _gridBoard.ItemCount);
    }

    [Test]
    public void Clear_RemovesAllItems()
    {
        var itemShape1 = Shapes.ImmutableSquare(2);
        var itemShape2 = Shapes.ImmutableLine(3);

        _gridBoard.TryAddItemAt(itemShape1, GridPosition.Zero);
        _gridBoard.TryAddItemAt(itemShape2, new GridPosition(2, 0));

        Assert.AreEqual(2, _gridBoard.ItemCount);

        _gridBoard.Clear();

        Assert.AreEqual(0, _gridBoard.ItemCount);
        Assert.AreEqual(100, _gridBoard.FreeSpace);
    }

    #endregion

    #region Clone Tests

    [Test]
    public void Clone_CreatesIdenticalCopy()
    {
        var itemShape = Shapes.ImmutableSquare(2);

        _gridBoard.TryAddItemAt(itemShape, new GridPosition(3, 4));

        var clone = _gridBoard.Clone(Allocator.Temp);

        Assert.AreEqual(_gridBoard.ItemCount, clone.ItemCount);
        Assert.AreEqual(_gridBoard.FreeSpace, clone.FreeSpace);
        Assert.AreEqual(_gridBoard.Width, clone.Width);
        Assert.AreEqual(_gridBoard.Height, clone.Height);

        for (var y = 0; y < 10; y++)
        for (var x = 0; x < 10; x++)
        {
            var pos = new GridPosition(x, y);
            Assert.AreEqual(_gridBoard.IsCellOccupied(pos), clone.IsCellOccupied(pos));
        }

        clone.Dispose();
    }

    [Test]
    public void Clone_ModificationsDoNotAffectOriginal()
    {
        var itemShape = Shapes.ImmutableSquare(2);

        _gridBoard.TryAddItemAt(itemShape, new GridPosition(0, 0));
        var clone = _gridBoard.Clone(Allocator.Temp);

        clone.RemoveItem(0);

        Assert.AreEqual(1, _gridBoard.ItemCount);
        Assert.AreEqual(0, clone.ItemCount);
        Assert.IsTrue(_gridBoard.IsCellOccupied(new GridPosition(0, 0)));
        Assert.IsFalse(clone.IsCellOccupied(new GridPosition(0, 0)));

        clone.Dispose();
    }

    #endregion

    #region Complex Shape Tests

    [Test]
    public void TryAddItem_LShapedItem()
    {
        var lShape = Shapes.ImmutableLShape();

        var index = _gridBoard.TryAddItemAt(lShape, GridPosition.Zero);

        Assert.IsTrue(index >= 0);
        Assert.AreEqual(1, _gridBoard.ItemCount);
        Assert.AreEqual(97, _gridBoard.FreeSpace); // L shape has 3 cells
    }

    [Test]
    public void TryAddItem_HollowSquare()
    {
        // Create a 3x3 hollow square manually since we don't have a premade one
        var hollowSquare = new GridShape(3, 3, Allocator.Temp);
        hollowSquare[0, 0] = true;
        hollowSquare[1, 0] = true;
        hollowSquare[2, 0] = true;
        hollowSquare[0, 1] = true;
        hollowSquare[2, 1] = true;
        hollowSquare[0, 2] = true;
        hollowSquare[1, 2] = true;
        hollowSquare[2, 2] = true;
        var immutableHollow = hollowSquare.GetOrCreateImmutable();
        hollowSquare.Dispose();

        var index = _gridBoard.TryAddItemAt(immutableHollow, GridPosition.Zero);

        Assert.IsTrue(index >= 0);
        Assert.AreEqual(1, _gridBoard.ItemCount);
        Assert.AreEqual(92, _gridBoard.FreeSpace);
    }

    #endregion

    #region Multiple Items Tests

    [Test]
    public void TryAddItem_MultipleItems_FillsSpace()
    {
        var smallItem = Shapes.ImmutableSingle();

        var addedCount = 0;
        for (var i = 0; i < 100; i++)
        {
            var x = i % 10;
            var y = i / 10;
            var index = _gridBoard.TryAddItemAt(smallItem, new GridPosition(x, y));
            if (index >= 0)
                addedCount++;
        }

        Assert.AreEqual(100, addedCount);
        Assert.AreEqual(100, _gridBoard.ItemCount);
        Assert.AreEqual(0, _gridBoard.FreeSpace);

        var extraIndex = _gridBoard.TryAddItemAt(smallItem, GridPosition.Zero);
        Assert.IsTrue(extraIndex < 0);
    }

    [Test]
    public void TryAddItem_Tetris_PackingTest()
    {
        var block2x2 = Shapes.ImmutableSquare(2);
        var line4x1 = Shapes.ImmutableLine(4);

        // Create vertical line manually
        var line1x4 = new GridShape(1, 4, Allocator.Temp);
        for (var y = 0; y < 4; y++)
            line1x4[0, y] = true;
        var immutableLine1x4 = line1x4.GetOrCreateImmutable();
        line1x4.Dispose();

        _gridBoard.TryAddItemAt(block2x2, GridPosition.Zero);
        _gridBoard.TryAddItemAt(line4x1, new GridPosition(2, 0));
        _gridBoard.TryAddItemAt(immutableLine1x4, new GridPosition(6, 0));

        Assert.AreEqual(3, _gridBoard.ItemCount);
        Assert.AreEqual(88, _gridBoard.FreeSpace);
    }

    [Test]
    public void RemoveItem_WithMultipleItems_CorrectlyUpdatesCount()
    {
        var item1 = CreateSquareShape(2, Allocator.Temp);
        var item2 = CreateSquareShape(3, Allocator.Temp);
        var item3 = CreateSquareShape(1, Allocator.Temp);

        _gridBoard.TryAddItemAt(item1.GetOrCreateImmutable(), GridPosition.Zero);
        _gridBoard.TryAddItemAt(item2.GetOrCreateImmutable(), new GridPosition(3, 0));
        _gridBoard.TryAddItemAt(item3.GetOrCreateImmutable(), new GridPosition(7, 0));

        Assert.AreEqual(3, _gridBoard.ItemCount);

        _gridBoard.RemoveItem(1);

        Assert.AreEqual(2, _gridBoard.ItemCount);

        item1.Dispose();
        item2.Dispose();
        item3.Dispose();
    }

    #endregion

    #region Boundary Tests

    [Test]
    public void TryAddItem_ItemLargerThanInventory_Fails()
    {
        var largeItem = new GridShape(11, 11, Allocator.Temp);
        for (var y = 0; y < 11; y++)
        for (var x = 0; x < 11; x++)
            largeItem[x, y] = true;

        var index = _gridBoard.TryAddItemAt(largeItem.GetOrCreateImmutable(), GridPosition.Zero);

        Assert.IsTrue(index < 0);
        Assert.AreEqual(0, _gridBoard.ItemCount);

        largeItem.Dispose();
    }

    [Test]
    public void TryAddItem_ExactFitItem()
    {
        var exactFit = new GridShape(10, 10, Allocator.Temp);
        for (var y = 0; y < 10; y++)
        for (var x = 0; x < 10; x++)
            exactFit[x, y] = true;

        var index = _gridBoard.TryAddItemAt(exactFit.GetOrCreateImmutable(), GridPosition.Zero);

        Assert.IsTrue(index >= 0);
        Assert.AreEqual(1, _gridBoard.ItemCount);
        Assert.AreEqual(0, _gridBoard.FreeSpace);

        exactFit.Dispose();
    }

    [Test]
    public void TryAddItemAt_NegativePosition_Fails()
    {
        var item = CreateSquareShape(2, Allocator.Temp);

        var index = _gridBoard.TryAddItemAt(item.GetOrCreateImmutable(), new GridPosition(-1, 0));
        Assert.IsTrue(index < 0);

        index = _gridBoard.TryAddItemAt(item.GetOrCreateImmutable(), new GridPosition(0, -1));
        Assert.IsTrue(index < 0);

        item.Dispose();
    }

    #endregion
}
