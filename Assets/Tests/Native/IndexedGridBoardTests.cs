using DopeGrid;
using DopeGrid.Native;
using NUnit.Framework;
using Unity.Collections;
using Unity.Mathematics;

public class IndexedGridBoardTests
{
    [Test]
    public void Constructor_CreatesEmptyBoard()
    {
        var board = new IndexedGridBoard(5, 5, Allocator.Temp);

        Assert.AreEqual(5, board.Width);
        Assert.AreEqual(5, board.Height);
        Assert.AreEqual(0, board.ItemCount);
        Assert.AreEqual(25, board.FreeSpace);

        // Check all cells are empty (-1)
        for (int y = 0; y < 5; y++)
        {
            for (int x = 0; x < 5; x++)
            {
                Assert.AreEqual(-1, board.GetItemIndexAt(new GridPosition(x, y)));
            }
        }

        board.Dispose();
    }

    [Test]
    public void TryAddItemAt_AddsItemAtPosition()
    {
        var board = new IndexedGridBoard(5, 5, Allocator.Temp);

        var shape = new GridShape(2, 2, Allocator.Temp);
        shape.SetCell(new int2(0, 0), true);
        shape.SetCell(new int2(1, 0), true);
        shape.SetCell(new int2(0, 1), true);
        shape.SetCell(new int2(1, 1), true);

        var index = board.TryAddItemAt(shape.GetOrCreateImmutable(), new GridPosition(1, 1));

        Assert.AreEqual(0, index);
        Assert.AreEqual(1, board.ItemCount);
        Assert.AreEqual(21, board.FreeSpace);

        // Check item index is stored at correct positions
        Assert.AreEqual(0, board.GetItemIndexAt(new GridPosition(1, 1)));
        Assert.AreEqual(0, board.GetItemIndexAt(new GridPosition(2, 1)));
        Assert.AreEqual(0, board.GetItemIndexAt(new GridPosition(1, 2)));
        Assert.AreEqual(0, board.GetItemIndexAt(new GridPosition(2, 2)));

        // Check adjacent cells are still empty
        Assert.AreEqual(-1, board.GetItemIndexAt(new GridPosition(0, 1)));
        Assert.AreEqual(-1, board.GetItemIndexAt(new GridPosition(3, 1)));

        shape.Dispose();
        board.Dispose();
    }

    [Test]
    public void TryAddItemAt_FailsWhenOverlapping()
    {
        var board = new IndexedGridBoard(5, 5, Allocator.Temp);

        var shape = new GridShape(2, 2, Allocator.Temp);
        for (int y = 0; y < 2; y++)
        for (int x = 0; x < 2; x++)
            shape.SetCell(new int2(x, y), true);

        var immutableShape = shape.GetOrCreateImmutable();

        var index1 = board.TryAddItemAt(immutableShape, new GridPosition(1, 1));
        var index2 = board.TryAddItemAt(immutableShape, new GridPosition(2, 2)); // Overlaps

        Assert.AreEqual(0, index1);
        Assert.AreEqual(-1, index2);
        Assert.AreEqual(1, board.ItemCount);

        shape.Dispose();
        board.Dispose();
    }

    [Test]
    public void RemoveItem_RemovesItemAndClearsGrid()
    {
        var board = new IndexedGridBoard(5, 5, Allocator.Temp);

        var shape = new GridShape(2, 2, Allocator.Temp);
        for (int y = 0; y < 2; y++)
        for (int x = 0; x < 2; x++)
            shape.SetCell(new int2(x, y), true);

        var index = board.TryAddItemAt(shape.GetOrCreateImmutable(), new GridPosition(1, 1));
        Assert.AreEqual(0, index);

        board.RemoveItem(index);

        Assert.AreEqual(0, board.ItemCount);
        Assert.AreEqual(25, board.FreeSpace);

        // Check cells are now empty
        Assert.AreEqual(-1, board.GetItemIndexAt(new GridPosition(1, 1)));
        Assert.AreEqual(-1, board.GetItemIndexAt(new GridPosition(2, 1)));
        Assert.AreEqual(-1, board.GetItemIndexAt(new GridPosition(1, 2)));
        Assert.AreEqual(-1, board.GetItemIndexAt(new GridPosition(2, 2)));

        shape.Dispose();
        board.Dispose();
    }

    [Test]
    public void StableIndices_IndicesDoNotChangeAfterRemoval()
    {
        var board = new IndexedGridBoard(10, 10, Allocator.Temp);

        var shape = new GridShape(2, 2, Allocator.Temp);
        for (int y = 0; y < 2; y++)
        for (int x = 0; x < 2; x++)
            shape.SetCell(new int2(x, y), true);
        var immutableShape = shape.GetOrCreateImmutable();

        // Add three items
        var index0 = board.TryAddItemAt(immutableShape, new GridPosition(0, 0));
        var index1 = board.TryAddItemAt(immutableShape, new GridPosition(3, 0));
        var index2 = board.TryAddItemAt(immutableShape, new GridPosition(6, 0));

        Assert.AreEqual(0, index0);
        Assert.AreEqual(1, index1);
        Assert.AreEqual(2, index2);

        // Remove middle item
        board.RemoveItem(index1);

        // Check that remaining items still have their original indices
        Assert.AreEqual(0, board.GetItemIndexAt(new GridPosition(0, 0)));
        Assert.AreEqual(2, board.GetItemIndexAt(new GridPosition(6, 0)));

        // Check middle item cells are now empty
        Assert.AreEqual(-1, board.GetItemIndexAt(new GridPosition(3, 0)));

        // Add another item - should reuse freed index
        var index3 = board.TryAddItemAt(immutableShape, new GridPosition(3, 3));
        Assert.AreEqual(1, index3); // Reused index 1

        shape.Dispose();
        board.Dispose();
    }

    [Test]
    public void TryAddItem_FindsFirstFitWithRotation()
    {
        var board = new IndexedGridBoard(5, 5, Allocator.Temp);

        // Block some cells
        var blockShape = new GridShape(3, 1, Allocator.Temp);
        blockShape.SetCell(new int2(0, 0), true);
        blockShape.SetCell(new int2(1, 0), true);
        blockShape.SetCell(new int2(2, 0), true);
        board.TryAddItemAt(blockShape.GetOrCreateImmutable(), new GridPosition(0, 0));

        // Try to add 1x3 vertical item (will need rotation to fit)
        var item = new GridShape(1, 3, Allocator.Temp);
        item.SetCell(new int2(0, 0), true);
        item.SetCell(new int2(0, 1), true);
        item.SetCell(new int2(0, 2), true);

        var (index, rotation) = board.TryAddItem(item.GetOrCreateImmutable());

        Assert.IsTrue(index >= 0);
        Assert.AreEqual(2, board.ItemCount);

        blockShape.Dispose();
        item.Dispose();
        board.Dispose();
    }

    [Test]
    public void GetItemShape_ReturnsCorrectShape()
    {
        var board = new IndexedGridBoard(5, 5, Allocator.Temp);

        var shape = new GridShape(2, 3, Allocator.Temp);
        shape.SetCell(new int2(0, 0), true);
        shape.SetCell(new int2(1, 1), true);
        shape.SetCell(new int2(0, 2), true);
        var immutableShape = shape.GetOrCreateImmutable();

        var index = board.TryAddItemAt(immutableShape, new GridPosition(1, 1));
        var retrievedShape = board.GetItemShape(index);

        Assert.AreEqual(immutableShape, retrievedShape);

        shape.Dispose();
        board.Dispose();
    }

    [Test]
    public void GetItemPosition_ReturnsCorrectPosition()
    {
        var board = new IndexedGridBoard(5, 5, Allocator.Temp);

        var shape = new GridShape(2, 2, Allocator.Temp);
        for (int y = 0; y < 2; y++)
        for (int x = 0; x < 2; x++)
            shape.SetCell(new int2(x, y), true);

        var index = board.TryAddItemAt(shape.GetOrCreateImmutable(), new GridPosition(2, 3));
        var position = board.GetItemPosition(index);

        Assert.AreEqual(2, position.X);
        Assert.AreEqual(3, position.Y);

        shape.Dispose();
        board.Dispose();
    }

    [Test]
    public void IsCellOccupied_ReturnsCorrectStatus()
    {
        var board = new IndexedGridBoard(5, 5, Allocator.Temp);

        var shape = new GridShape(2, 2, Allocator.Temp);
        for (int y = 0; y < 2; y++)
        for (int x = 0; x < 2; x++)
            shape.SetCell(new int2(x, y), true);

        board.TryAddItemAt(shape.GetOrCreateImmutable(), new GridPosition(1, 1));

        Assert.IsTrue(board.IsCellOccupied(new GridPosition(1, 1)));
        Assert.IsTrue(board.IsCellOccupied(new GridPosition(2, 2)));
        Assert.IsFalse(board.IsCellOccupied(new GridPosition(0, 0)));
        Assert.IsFalse(board.IsCellOccupied(new GridPosition(3, 3)));

        shape.Dispose();
        board.Dispose();
    }

    [Test]
    public void Clear_ResetsBoard()
    {
        var board = new IndexedGridBoard(5, 5, Allocator.Temp);

        var shape = new GridShape(2, 2, Allocator.Temp);
        for (int y = 0; y < 2; y++)
        for (int x = 0; x < 2; x++)
            shape.SetCell(new int2(x, y), true);

        board.TryAddItemAt(shape.GetOrCreateImmutable(), new GridPosition(1, 1));
        Assert.AreEqual(1, board.ItemCount);

        board.Clear();

        Assert.AreEqual(0, board.ItemCount);
        Assert.AreEqual(25, board.FreeSpace);
        Assert.IsFalse(board.IsCellOccupied(new GridPosition(1, 1)));

        shape.Dispose();
        board.Dispose();
    }
}
