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
            shape.SetCellValue(new GridPosition(x, y), true);
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

        var (index, rotation) = _gridBoard.TryAddItem(itemShape);

        Assert.AreEqual(0, index);
        Assert.AreEqual(1, _gridBoard.ItemCount);
        Assert.AreEqual(96, _gridBoard.FreeSpace);
    }

    [Test]
    public void TryAddItemAt_AddsItemAtSpecificPosition()
    {
        var itemShape = Shapes.ImmutableSquare(2);

        var index = _gridBoard.TryAddItemAt(itemShape, new GridPosition(3, 4));

        Assert.AreEqual(0, index);
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

        Assert.AreEqual(-1, secondAdd);
        Assert.AreEqual(1, _gridBoard.ItemCount);
    }

    [Test]
    public void TryAddItemAt_FailsWhenOutOfBounds()
    {
        var itemShape = Shapes.ImmutableSingle();

        var index = _gridBoard.TryAddItemAt(itemShape, new GridPosition(10, 10));

        Assert.AreEqual(-1, index);
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

        var (_, _) = _gridBoard.TryAddItem(itemShape1);
        var (_, _) = _gridBoard.TryAddItem(itemShape2);

        Assert.AreEqual(2, _gridBoard.ItemCount);

        _gridBoard.Clear();

        Assert.AreEqual(0, _gridBoard.ItemCount);
        Assert.AreEqual(100, _gridBoard.FreeSpace);
    }

    [Test]
    public void GetItemShape_ReturnsAddedItems()
    {
        var item1 = Shapes.ImmutableSquare(2);
        var item2 = Shapes.ImmutableSingle();

        var (index1, rotation1) = _gridBoard.TryAddItem(item1);
        var (index2, rotation2) = _gridBoard.TryAddItem(item2);

        Assert.AreEqual(2, _gridBoard.ItemCount);
        Assert.AreEqual(item1.GetRotatedShape(rotation1), _gridBoard.GetItemShape(index1));
        Assert.AreEqual(item2.GetRotatedShape(rotation2), _gridBoard.GetItemShape(index2));
    }

    [Test]
    public void GetItemPosition_ReturnsCorrectPositions()
    {
        var item1 = Shapes.ImmutableSquare(2);
        var item2 = Shapes.ImmutableSingle();

        var index1 = _gridBoard.TryAddItemAt(item1, new GridPosition(1, 2));
        var index2 = _gridBoard.TryAddItemAt(item2, new GridPosition(5, 6));

        Assert.AreEqual(2, _gridBoard.ItemCount);
        Assert.AreEqual(new GridPosition(1, 2), _gridBoard.GetItemPosition(index1));
        Assert.AreEqual(new GridPosition(5, 6), _gridBoard.GetItemPosition(index2));
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

        var (index, rotation) = _gridBoard.TryAddItem(lShape);

        Assert.AreEqual(0, index);
        Assert.AreEqual(1, _gridBoard.ItemCount);
        Assert.AreEqual(97, _gridBoard.FreeSpace); // L shape has 3 cells
    }

    [Test]
    public void TryAddItem_HollowSquare()
    {
        // Create a 3x3 hollow square manually since we don't have a premade one
        var hollowSquare = new GridShape(3, 3, Allocator.Temp);
        hollowSquare.SetCellValue(new GridPosition(0, 0), true);
        hollowSquare.SetCellValue(new GridPosition(1, 0), true);
        hollowSquare.SetCellValue(new GridPosition(2, 0), true);
        hollowSquare.SetCellValue(new GridPosition(0, 1), true);
        hollowSquare.SetCellValue(new GridPosition(2, 1), true);
        hollowSquare.SetCellValue(new GridPosition(0, 2), true);
        hollowSquare.SetCellValue(new GridPosition(1, 2), true);
        hollowSquare.SetCellValue(new GridPosition(2, 2), true);
        var immutableHollow = hollowSquare.GetOrCreateImmutable();
        hollowSquare.Dispose();

        var (index, rotation) = _gridBoard.TryAddItem(immutableHollow);

        Assert.AreEqual(0, index);
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
            var (index, _) = _gridBoard.TryAddItem(smallItem);
            if (index >= 0)
                addedCount++;
        }

        Assert.AreEqual(100, addedCount);
        Assert.AreEqual(100, _gridBoard.ItemCount);
        Assert.AreEqual(0, _gridBoard.FreeSpace);

        var (extraIndex, _) = _gridBoard.TryAddItem(smallItem);
        Assert.AreEqual(-1, extraIndex);
    }

    [Test]
    public void TryAddItem_Tetris_PackingTest()
    {
        var block2x2 = Shapes.ImmutableSquare(2);
        var line4x1 = Shapes.ImmutableLine(4);

        // Create vertical line manually
        var line1x4 = new GridShape(1, 4, Allocator.Temp);
        for (var y = 0; y < 4; y++)
            line1x4.SetCellValue(new GridPosition(0, y), true);
        var immutableLine1x4 = line1x4.GetOrCreateImmutable();
        line1x4.Dispose();

        var (_, _) = _gridBoard.TryAddItem(block2x2);
        var (_, _) = _gridBoard.TryAddItem(line4x1);
        var (_, _) = _gridBoard.TryAddItem(immutableLine1x4);

        Assert.AreEqual(3, _gridBoard.ItemCount);
        Assert.AreEqual(88, _gridBoard.FreeSpace);
    }

    [Test]
    public void RemoveItem_WithMultipleItems_CorrectlyUpdatesCount()
    {
        var item1 = CreateSquareShape(2, Allocator.Temp);
        var item2 = CreateSquareShape(3, Allocator.Temp);
        var item3 = CreateSquareShape(1, Allocator.Temp);

        var (index1, _) = _gridBoard.TryAddItem(item1.GetOrCreateImmutable());
        var (index2, _) = _gridBoard.TryAddItem(item2.GetOrCreateImmutable());
        var (index3, _) = _gridBoard.TryAddItem(item3.GetOrCreateImmutable());

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
            largeItem.SetCellValue(x, y, true);

        var (index, rotation) = _gridBoard.TryAddItem(largeItem.GetOrCreateImmutable());

        Assert.AreEqual(-1, index);
        Assert.AreEqual(0, _gridBoard.ItemCount);

        largeItem.Dispose();
    }

    [Test]
    public void TryAddItem_ExactFitItem()
    {
        var exactFit = new GridShape(10, 10, Allocator.Temp);
        for (var y = 0; y < 10; y++)
        for (var x = 0; x < 10; x++)
            exactFit.SetCellValue(new GridPosition(x, y), true);

        var (index, rotation) = _gridBoard.TryAddItem(exactFit.GetOrCreateImmutable());

        Assert.AreEqual(0, index);
        Assert.AreEqual(1, _gridBoard.ItemCount);
        Assert.AreEqual(0, _gridBoard.FreeSpace);

        exactFit.Dispose();
    }

    [Test]
    public void TryAddItemAt_NegativePosition_Fails()
    {
        var item = CreateSquareShape(2, Allocator.Temp);

        var index = _gridBoard.TryAddItemAt(item.GetOrCreateImmutable(), new GridPosition(-1, 0));
        Assert.AreEqual(-1, index);

        index = _gridBoard.TryAddItemAt(item.GetOrCreateImmutable(), new GridPosition(0, -1));
        Assert.AreEqual(-1, index);

        item.Dispose();
    }

    #endregion

    #region ReadOnly Tests

    [Test]
    public void AsReadOnly_ProvidesReadOnlyAccess()
    {
        var shape = Shapes.ImmutableSquare(2);
        var index = _gridBoard.TryAddItemAt(shape, new GridPosition(2, 3));

        var readOnly = _gridBoard.AsReadOnly();

        Assert.AreEqual(_gridBoard.Width, readOnly.Width);
        Assert.AreEqual(_gridBoard.Height, readOnly.Height);
        Assert.AreEqual(_gridBoard.ItemCount, readOnly.ItemCount);
        Assert.AreEqual(_gridBoard.FreeSpace, readOnly.FreeSpace);
        Assert.AreEqual(shape, readOnly.GetItemShape(index));
        Assert.AreEqual(new GridPosition(2, 3), readOnly.GetItemPosition(index));
        Assert.IsTrue(readOnly.IsCellOccupied(new GridPosition(2, 3)));
        Assert.IsFalse(readOnly.IsCellOccupied(new GridPosition(0, 0)));
    }

    [Test]
    public void ImplicitConversion_ToReadOnly()
    {
        var shape = Shapes.ImmutableSquare(2);
        _gridBoard.TryAddItemAt(shape, new GridPosition(1, 1));

        GridBoard.ReadOnly readOnly = _gridBoard; // Implicit conversion

        Assert.AreEqual(1, readOnly.ItemCount);
        Assert.IsTrue(readOnly.IsCellOccupied(new GridPosition(1, 1)));
    }

    #endregion
}
