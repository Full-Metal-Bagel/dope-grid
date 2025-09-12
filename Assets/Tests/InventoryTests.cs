using DopeInventory;
using NUnit.Framework;
using Unity.Collections;
using Unity.Mathematics;

public class InventoryTests
{
    private Inventory _inventory;

    [SetUp]
    public void Setup()
    {
        _inventory = new Inventory(10, 10, Allocator.Temp);
    }

    [TearDown]
    public void TearDown()
    {
        if (_inventory.InventoryGrid.IsCreated)
            _inventory.Dispose();
    }

    #region Helper Methods

    private GridShape2D CreateSquareShape(int size, Allocator allocator)
    {
        var shape = new GridShape2D(size, size, allocator);
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
        Assert.AreEqual(10, _inventory.InventoryGrid.Width);
        Assert.AreEqual(10, _inventory.InventoryGrid.Height);
        Assert.AreEqual(0, _inventory.ItemCount);
        Assert.AreEqual(100, _inventory.FreeSpace);
    }

    [Test]
    public void TryAddItem_AddsItemSuccessfully()
    {
        var itemShape = new GridShape2D(2, 2, Allocator.Temp);
        itemShape.SetCell(new int2(0, 0), true);
        itemShape.SetCell(new int2(1, 0), true);
        itemShape.SetCell(new int2(0, 1), true);
        itemShape.SetCell(new int2(1, 1), true);

        var added = _inventory.TryAddItem(itemShape);

        Assert.IsTrue(added);
        Assert.AreEqual(1, _inventory.ItemCount);
        Assert.AreEqual(96, _inventory.FreeSpace);

        itemShape.Dispose();
    }

    [Test]
    public void TryAddItemAt_AddsItemAtSpecificPosition()
    {
        var itemShape = new GridShape2D(2, 2, Allocator.Temp);
        itemShape.SetCell(new int2(0, 0), true);
        itemShape.SetCell(new int2(1, 0), true);
        itemShape.SetCell(new int2(0, 1), true);
        itemShape.SetCell(new int2(1, 1), true);

        var added = _inventory.TryAddItemAt(itemShape, new int2(3, 4));

        Assert.IsTrue(added);
        Assert.AreEqual(1, _inventory.ItemCount);
        Assert.IsTrue(_inventory.IsCellOccupied(new int2(3, 4)));
        Assert.IsTrue(_inventory.IsCellOccupied(new int2(4, 4)));
        Assert.IsTrue(_inventory.IsCellOccupied(new int2(3, 5)));
        Assert.IsTrue(_inventory.IsCellOccupied(new int2(4, 5)));

        itemShape.Dispose();
    }

    [Test]
    public void TryAddItemAt_FailsWhenPositionOccupied()
    {
        var itemShape = new GridShape2D(2, 2, Allocator.Temp);
        itemShape.SetCell(new int2(0, 0), true);
        itemShape.SetCell(new int2(1, 0), true);
        itemShape.SetCell(new int2(0, 1), true);
        itemShape.SetCell(new int2(1, 1), true);

        _inventory.TryAddItemAt(itemShape, new int2(3, 4));
        var secondAdd = _inventory.TryAddItemAt(itemShape, new int2(3, 4));

        Assert.IsFalse(secondAdd);
        Assert.AreEqual(1, _inventory.ItemCount);

        itemShape.Dispose();
    }

    [Test]
    public void TryAddItemAt_FailsWhenOutOfBounds()
    {
        var itemShape = new GridShape2D(3, 3, Allocator.Temp);
        itemShape.SetCell(new int2(0, 0), true);

        var added = _inventory.TryAddItemAt(itemShape, new int2(8, 8));

        Assert.IsFalse(added);
        Assert.AreEqual(0, _inventory.ItemCount);

        itemShape.Dispose();
    }

    [Test]
    public void RemoveItem_RemovesItemSuccessfully()
    {
        var itemShape = new GridShape2D(2, 2, Allocator.Temp);
        itemShape.SetCell(new int2(0, 0), true);
        itemShape.SetCell(new int2(1, 0), true);
        itemShape.SetCell(new int2(0, 1), true);
        itemShape.SetCell(new int2(1, 1), true);

        _inventory.TryAddItemAt(itemShape, new int2(2, 2));
        _inventory.RemoveItem(0);

        Assert.AreEqual(0, _inventory.ItemCount);
        Assert.AreEqual(100, _inventory.FreeSpace);
        Assert.IsFalse(_inventory.IsCellOccupied(new int2(2, 2)));
        Assert.IsFalse(_inventory.IsCellOccupied(new int2(3, 2)));
        Assert.IsFalse(_inventory.IsCellOccupied(new int2(2, 3)));
        Assert.IsFalse(_inventory.IsCellOccupied(new int2(3, 3)));

        itemShape.Dispose();
    }

    [Test]
    public void RemoveItem_HandlesInvalidIndex()
    {
        _inventory.RemoveItem(-1);
        _inventory.RemoveItem(10);

        Assert.AreEqual(0, _inventory.ItemCount);
    }

    [Test]
    public void Clear_RemovesAllItems()
    {
        var itemShape1 = new GridShape2D(2, 2, Allocator.Temp);
        var itemShape2 = new GridShape2D(3, 1, Allocator.Temp);

        itemShape1.SetCell(new int2(0, 0), true);
        itemShape1.SetCell(new int2(1, 0), true);
        itemShape2.SetCell(new int2(0, 0), true);
        itemShape2.SetCell(new int2(1, 0), true);
        itemShape2.SetCell(new int2(2, 0), true);

        _inventory.TryAddItem(itemShape1);
        _inventory.TryAddItem(itemShape2);

        Assert.AreEqual(2, _inventory.ItemCount);

        _inventory.Clear();

        Assert.AreEqual(0, _inventory.ItemCount);
        Assert.AreEqual(100, _inventory.FreeSpace);

        itemShape1.Dispose();
        itemShape2.Dispose();
    }

    #endregion

    #region Clone Tests

    [Test]
    public void Clone_CreatesIdenticalCopy()
    {
        var itemShape = new GridShape2D(2, 2, Allocator.Temp);
        itemShape.SetCell(new int2(0, 0), true);
        itemShape.SetCell(new int2(1, 0), true);
        itemShape.SetCell(new int2(0, 1), true);
        itemShape.SetCell(new int2(1, 1), true);

        _inventory.TryAddItemAt(itemShape, new int2(3, 4));

        var clone = _inventory.Clone(Allocator.Temp);

        Assert.AreEqual(_inventory.ItemCount, clone.ItemCount);
        Assert.AreEqual(_inventory.FreeSpace, clone.FreeSpace);
        Assert.AreEqual(_inventory.InventoryGrid.Width, clone.InventoryGrid.Width);
        Assert.AreEqual(_inventory.InventoryGrid.Height, clone.InventoryGrid.Height);

        for (var y = 0; y < 10; y++)
        for (var x = 0; x < 10; x++)
        {
            var pos = new int2(x, y);
            Assert.AreEqual(_inventory.IsCellOccupied(pos), clone.IsCellOccupied(pos));
        }

        itemShape.Dispose();
        clone.Dispose();
    }

    [Test]
    public void Clone_ModificationsDoNotAffectOriginal()
    {
        var itemShape = new GridShape2D(2, 2, Allocator.Temp);
        itemShape.SetCell(new int2(0, 0), true);
        itemShape.SetCell(new int2(1, 0), true);
        itemShape.SetCell(new int2(0, 1), true);
        itemShape.SetCell(new int2(1, 1), true);

        _inventory.TryAddItemAt(itemShape, new int2(0, 0));
        var clone = _inventory.Clone(Allocator.Temp);

        clone.RemoveItem(0);

        Assert.AreEqual(1, _inventory.ItemCount);
        Assert.AreEqual(0, clone.ItemCount);
        Assert.IsTrue(_inventory.IsCellOccupied(new int2(0, 0)));
        Assert.IsFalse(clone.IsCellOccupied(new int2(0, 0)));

        itemShape.Dispose();
        clone.Dispose();
    }

    #endregion

    #region Complex Shape Tests

    [Test]
    public void TryAddItem_LShapedItem()
    {
        var lShape = new GridShape2D(2, 3, Allocator.Temp);
        lShape.SetCell(new int2(0, 0), true);
        lShape.SetCell(new int2(0, 1), true);
        lShape.SetCell(new int2(0, 2), true);
        lShape.SetCell(new int2(1, 2), true);

        var added = _inventory.TryAddItem(lShape);

        Assert.IsTrue(added);
        Assert.AreEqual(1, _inventory.ItemCount);
        Assert.AreEqual(96, _inventory.FreeSpace);

        lShape.Dispose();
    }

    [Test]
    public void TryAddItem_HollowSquare()
    {
        var hollowSquare = new GridShape2D(3, 3, Allocator.Temp);
        hollowSquare.SetCell(new int2(0, 0), true);
        hollowSquare.SetCell(new int2(1, 0), true);
        hollowSquare.SetCell(new int2(2, 0), true);
        hollowSquare.SetCell(new int2(0, 1), true);
        hollowSquare.SetCell(new int2(2, 1), true);
        hollowSquare.SetCell(new int2(0, 2), true);
        hollowSquare.SetCell(new int2(1, 2), true);
        hollowSquare.SetCell(new int2(2, 2), true);

        var added = _inventory.TryAddItem(hollowSquare);

        Assert.IsTrue(added);
        Assert.AreEqual(1, _inventory.ItemCount);
        Assert.AreEqual(92, _inventory.FreeSpace);

        hollowSquare.Dispose();
    }

    #endregion

    #region Multiple Items Tests

    [Test]
    public void TryAddItem_MultipleItems_FillsSpace()
    {
        var smallItem = new GridShape2D(1, 1, Allocator.Temp);
        smallItem.SetCell(new int2(0, 0), true);

        var addedCount = 0;
        for (var i = 0; i < 100; i++)
            if (_inventory.TryAddItem(smallItem))
                addedCount++;

        Assert.AreEqual(100, addedCount);
        Assert.AreEqual(100, _inventory.ItemCount);
        Assert.AreEqual(0, _inventory.FreeSpace);

        var extraAdd = _inventory.TryAddItem(smallItem);
        Assert.IsFalse(extraAdd);

        smallItem.Dispose();
    }

    [Test]
    public void TryAddItem_Tetris_PackingTest()
    {
        var block2x2 = new GridShape2D(2, 2, Allocator.Temp);
        for (var y = 0; y < 2; y++)
        for (var x = 0; x < 2; x++)
            block2x2.SetCell(new int2(x, y), true);

        var line4x1 = new GridShape2D(4, 1, Allocator.Temp);
        for (var x = 0; x < 4; x++)
            line4x1.SetCell(new int2(x, 0), true);

        var line1x4 = new GridShape2D(1, 4, Allocator.Temp);
        for (var y = 0; y < 4; y++)
            line1x4.SetCell(new int2(0, y), true);

        _inventory.TryAddItem(block2x2);
        _inventory.TryAddItem(line4x1);
        _inventory.TryAddItem(line1x4);

        Assert.AreEqual(3, _inventory.ItemCount);
        Assert.AreEqual(88, _inventory.FreeSpace);

        block2x2.Dispose();
        line4x1.Dispose();
        line1x4.Dispose();
    }

    [Test]
    public void RemoveItem_WithMultipleItems_MaintainsOrder()
    {
        var item1 = CreateSquareShape(2, Allocator.Temp);
        var item2 = CreateSquareShape(3, Allocator.Temp);
        var item3 = CreateSquareShape(1, Allocator.Temp);

        _inventory.TryAddItem(item1);
        _inventory.TryAddItem(item2);
        _inventory.TryAddItem(item3);

        Assert.AreEqual(3, _inventory.ItemCount);

        _inventory.RemoveItem(1);

        Assert.AreEqual(2, _inventory.ItemCount);

        item1.Dispose();
        item2.Dispose();
        item3.Dispose();
    }

    #endregion

    #region Boundary Tests

    [Test]
    public void TryAddItem_ItemLargerThanInventory_Fails()
    {
        var largeItem = new GridShape2D(11, 11, Allocator.Temp);
        largeItem.SetCell(new int2(0, 0), true);

        var added = _inventory.TryAddItem(largeItem);

        Assert.IsFalse(added);
        Assert.AreEqual(0, _inventory.ItemCount);

        largeItem.Dispose();
    }

    [Test]
    public void TryAddItem_ExactFitItem()
    {
        var exactFit = new GridShape2D(10, 10, Allocator.Temp);
        for (var y = 0; y < 10; y++)
        for (var x = 0; x < 10; x++)
            exactFit.SetCell(new int2(x, y), true);

        var added = _inventory.TryAddItem(exactFit);

        Assert.IsTrue(added);
        Assert.AreEqual(1, _inventory.ItemCount);
        Assert.AreEqual(0, _inventory.FreeSpace);

        exactFit.Dispose();
    }

    [Test]
    public void TryAddItemAt_NegativePosition_Fails()
    {
        var item = CreateSquareShape(2, Allocator.Temp);

        var added = _inventory.TryAddItemAt(item, new int2(-1, 0));
        Assert.IsFalse(added);

        added = _inventory.TryAddItemAt(item, new int2(0, -1));
        Assert.IsFalse(added);

        item.Dispose();
    }

    #endregion
}