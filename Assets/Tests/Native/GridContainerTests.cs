using DopeGrid;
using DopeGrid.Native;
using NUnit.Framework;
using Unity.Collections;
using Unity.Mathematics;

public class GridContainerTests
{
    private GridContainer _gridContainer;

    [SetUp]
    public void Setup()
    {
        _gridContainer = new GridContainer(10, 10, Allocator.Temp);
    }

    [TearDown]
    public void TearDown()
    {
        _gridContainer.Dispose();
    }

    #region Helper Methods

    private GridShape CreateSquareShape(int size, Allocator allocator)
    {
        var shape = new GridShape(size, size, allocator);
        for (var y = 0; y < size; y++)
        for (var x = 0; x < size; x++)
            shape.SetCell(new int2(x, y), true);
        return shape;
    }

    #endregion

    #region Basic Inventory Tests

    [Test]
    public void Constructor_CreatesEmptyInventory()
    {
        Assert.AreEqual(10, _gridContainer.Width);
        Assert.AreEqual(10, _gridContainer.Height);
        Assert.AreEqual(0, _gridContainer.ItemCount);
        Assert.AreEqual(100, _gridContainer.FreeSpace);
    }

    [Test]
    public void TryAddItem_AddsItemSuccessfully()
    {
        var itemShape = Shapes.ImmutableSquare(2);

        var added = _gridContainer.TryAddItem(itemShape);

        Assert.IsTrue(added);
        Assert.AreEqual(1, _gridContainer.ItemCount);
        Assert.AreEqual(96, _gridContainer.FreeSpace);
    }

    [Test]
    public void TryAddItemAt_AddsItemAtSpecificPosition()
    {
        var itemShape = Shapes.ImmutableSquare(2);

        var added = _gridContainer.TryAddItemAt(itemShape, new int2(3, 4));

        Assert.IsTrue(added);
        Assert.AreEqual(1, _gridContainer.ItemCount);
        Assert.IsTrue(_gridContainer.IsCellOccupied(new int2(3, 4)));
        Assert.IsTrue(_gridContainer.IsCellOccupied(new int2(4, 4)));
        Assert.IsTrue(_gridContainer.IsCellOccupied(new int2(3, 5)));
        Assert.IsTrue(_gridContainer.IsCellOccupied(new int2(4, 5)));
    }

    [Test]
    public void TryAddItemAt_FailsWhenPositionOccupied()
    {
        var itemShape = Shapes.ImmutableSquare(2);

        _gridContainer.TryAddItemAt(itemShape, new int2(3, 4));
        var secondAdd = _gridContainer.TryAddItemAt(itemShape, new int2(3, 4));

        Assert.IsFalse(secondAdd);
        Assert.AreEqual(1, _gridContainer.ItemCount);
    }

    [Test]
    public void TryAddItemAt_FailsWhenOutOfBounds()
    {
        var itemShape = Shapes.ImmutableSingle();

        var added = _gridContainer.TryAddItemAt(itemShape, new int2(10, 10));

        Assert.IsFalse(added);
        Assert.AreEqual(0, _gridContainer.ItemCount);
    }

    [Test]
    public void RemoveItem_RemovesItemSuccessfully()
    {
        var itemShape = Shapes.ImmutableSquare(2);

        _gridContainer.TryAddItemAt(itemShape, new int2(2, 2));
        _gridContainer.RemoveItem(0);

        Assert.AreEqual(0, _gridContainer.ItemCount);
        Assert.AreEqual(100, _gridContainer.FreeSpace);
        Assert.IsFalse(_gridContainer.IsCellOccupied(new int2(2, 2)));
        Assert.IsFalse(_gridContainer.IsCellOccupied(new int2(3, 2)));
        Assert.IsFalse(_gridContainer.IsCellOccupied(new int2(2, 3)));
        Assert.IsFalse(_gridContainer.IsCellOccupied(new int2(3, 3)));
    }

    [Test]
    public void RemoveItem_HandlesInvalidIndex()
    {
        _gridContainer.RemoveItem(-1);
        _gridContainer.RemoveItem(10);

        Assert.AreEqual(0, _gridContainer.ItemCount);
    }

    [Test]
    public void Clear_RemovesAllItems()
    {
        var itemShape1 = Shapes.ImmutableSquare(2);
        var itemShape2 = Shapes.ImmutableLine(3);

        _gridContainer.TryAddItem(itemShape1);
        _gridContainer.TryAddItem(itemShape2);

        Assert.AreEqual(2, _gridContainer.ItemCount);

        _gridContainer.Clear();

        Assert.AreEqual(0, _gridContainer.ItemCount);
        Assert.AreEqual(100, _gridContainer.FreeSpace);
    }

    #endregion

    #region Clone Tests

    [Test]
    public void Clone_CreatesIdenticalCopy()
    {
        var itemShape = Shapes.ImmutableSquare(2);

        _gridContainer.TryAddItemAt(itemShape, new int2(3, 4));

        var clone = _gridContainer.Clone(Allocator.Temp);

        Assert.AreEqual(_gridContainer.ItemCount, clone.ItemCount);
        Assert.AreEqual(_gridContainer.FreeSpace, clone.FreeSpace);
        Assert.AreEqual(_gridContainer.Width, clone.Width);
        Assert.AreEqual(_gridContainer.Height, clone.Height);

        for (var y = 0; y < 10; y++)
        for (var x = 0; x < 10; x++)
        {
            var pos = new int2(x, y);
            Assert.AreEqual(_gridContainer.IsCellOccupied(pos), clone.IsCellOccupied(pos));
        }

        clone.Dispose();
    }

    [Test]
    public void Clone_ModificationsDoNotAffectOriginal()
    {
        var itemShape = Shapes.ImmutableSquare(2);

        _gridContainer.TryAddItemAt(itemShape, new int2(0, 0));
        var clone = _gridContainer.Clone(Allocator.Temp);

        clone.RemoveItem(0);

        Assert.AreEqual(1, _gridContainer.ItemCount);
        Assert.AreEqual(0, clone.ItemCount);
        Assert.IsTrue(_gridContainer.IsCellOccupied(new int2(0, 0)));
        Assert.IsFalse(clone.IsCellOccupied(new int2(0, 0)));

        clone.Dispose();
    }

    #endregion

    #region Complex Shape Tests

    [Test]
    public void TryAddItem_LShapedItem()
    {
        var lShape = Shapes.ImmutableLShape();

        var added = _gridContainer.TryAddItem(lShape);

        Assert.IsTrue(added);
        Assert.AreEqual(1, _gridContainer.ItemCount);
        Assert.AreEqual(97, _gridContainer.FreeSpace); // L shape has 3 cells
    }

    [Test]
    public void TryAddItem_HollowSquare()
    {
        // Create a 3x3 hollow square manually since we don't have a premade one
        var hollowSquare = new GridShape(3, 3, Allocator.Temp);
        hollowSquare.SetCell(new int2(0, 0), true);
        hollowSquare.SetCell(new int2(1, 0), true);
        hollowSquare.SetCell(new int2(2, 0), true);
        hollowSquare.SetCell(new int2(0, 1), true);
        hollowSquare.SetCell(new int2(2, 1), true);
        hollowSquare.SetCell(new int2(0, 2), true);
        hollowSquare.SetCell(new int2(1, 2), true);
        hollowSquare.SetCell(new int2(2, 2), true);
        var immutableHollow = hollowSquare.GetOrCreateImmutable();
        hollowSquare.Dispose();

        var added = _gridContainer.TryAddItem(immutableHollow);

        Assert.IsTrue(added);
        Assert.AreEqual(1, _gridContainer.ItemCount);
        Assert.AreEqual(92, _gridContainer.FreeSpace);
    }

    #endregion

    #region Multiple Items Tests

    [Test]
    public void TryAddItem_MultipleItems_FillsSpace()
    {
        var smallItem = Shapes.ImmutableSingle();

        var addedCount = 0;
        for (var i = 0; i < 100; i++)
            if (_gridContainer.TryAddItem(smallItem))
                addedCount++;

        Assert.AreEqual(100, addedCount);
        Assert.AreEqual(100, _gridContainer.ItemCount);
        Assert.AreEqual(0, _gridContainer.FreeSpace);

        var extraAdd = _gridContainer.TryAddItem(smallItem);
        Assert.IsFalse(extraAdd);
    }

    [Test]
    public void TryAddItem_Tetris_PackingTest()
    {
        var block2x2 = Shapes.ImmutableSquare(2);
        var line4x1 = Shapes.ImmutableLine(4);

        // Create vertical line manually
        var line1x4 = new GridShape(1, 4, Allocator.Temp);
        for (var y = 0; y < 4; y++)
            line1x4.SetCell(new int2(0, y), true);
        var immutableLine1x4 = line1x4.GetOrCreateImmutable();
        line1x4.Dispose();

        _gridContainer.TryAddItem(block2x2);
        _gridContainer.TryAddItem(line4x1);
        _gridContainer.TryAddItem(immutableLine1x4);

        Assert.AreEqual(3, _gridContainer.ItemCount);
        Assert.AreEqual(88, _gridContainer.FreeSpace);
    }

    [Test]
    public void RemoveItem_WithMultipleItems_MaintainsOrder()
    {
        var item1 = CreateSquareShape(2, Allocator.Temp);
        var item2 = CreateSquareShape(3, Allocator.Temp);
        var item3 = CreateSquareShape(1, Allocator.Temp);

        _gridContainer.TryAddItem(item1.GetOrCreateImmutable());
        _gridContainer.TryAddItem(item2.GetOrCreateImmutable());
        _gridContainer.TryAddItem(item3.GetOrCreateImmutable());

        Assert.AreEqual(3, _gridContainer.ItemCount);

        _gridContainer.RemoveItem(1);

        Assert.AreEqual(2, _gridContainer.ItemCount);

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
            largeItem.SetCell(x, y, true);

        var added = _gridContainer.TryAddItem(largeItem.GetOrCreateImmutable());

        Assert.IsFalse(added);
        Assert.AreEqual(0, _gridContainer.ItemCount);

        largeItem.Dispose();
    }

    [Test]
    public void TryAddItem_ExactFitItem()
    {
        var exactFit = new GridShape(10, 10, Allocator.Temp);
        for (var y = 0; y < 10; y++)
        for (var x = 0; x < 10; x++)
            exactFit.SetCell(new int2(x, y), true);

        var added = _gridContainer.TryAddItem(exactFit.GetOrCreateImmutable());

        Assert.IsTrue(added);
        Assert.AreEqual(1, _gridContainer.ItemCount);
        Assert.AreEqual(0, _gridContainer.FreeSpace);

        exactFit.Dispose();
    }

    [Test]
    public void TryAddItemAt_NegativePosition_Fails()
    {
        var item = CreateSquareShape(2, Allocator.Temp);

        var added = _gridContainer.TryAddItemAt(item.GetOrCreateImmutable(), new int2(-1, 0));
        Assert.IsFalse(added);

        added = _gridContainer.TryAddItemAt(item.GetOrCreateImmutable(), new int2(0, -1));
        Assert.IsFalse(added);

        item.Dispose();
    }

    #endregion
}
