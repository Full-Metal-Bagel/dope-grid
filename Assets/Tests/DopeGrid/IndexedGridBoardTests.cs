using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace DopeGrid.Tests;

[TestFixture]
public class IndexedGridBoardTests
{

    [Test]
    public void Constructor_WithDimensions_CreatesBoard()
    {
        using var board = new IndexedGridBoard(5, 5);

        Assert.That(board.Width, Is.EqualTo(5));
        Assert.That(board.Height, Is.EqualTo(5));
        Assert.That(board.ItemCount, Is.EqualTo(0));
    }

    [Test]
    public void CreateFromShape_WithGridShape_CreatesBoard()
    {
        using var shape = new GridShape(3, 3);
        shape.FillAll(true);
        using var board = IndexedGridBoard.CreateFromShape(shape);

        Assert.That(board.Width, Is.EqualTo(3));
        Assert.That(board.Height, Is.EqualTo(3));
    }

    [Test]
    public void CreateFromShape_WithEmptyShape_CreatesEmptyBoard()
    {
        using var shape = new GridShape(0, 0);
        using var board = IndexedGridBoard.CreateFromShape(shape);

        Assert.That(board.Width, Is.EqualTo(0));
        Assert.That(board.Height, Is.EqualTo(0));
    }

    [Test]
    public void TryAddItem_WithValidItem_ReturnsIndexAndRotation()
    {
        using var board = new IndexedGridBoard(5, 5);
        var shape = Shapes.ImmutableSingle();

        var (item, rotation) = board.TryAddItem(shape);

        Assert.That(item.Id, Is.GreaterThanOrEqualTo(0));
        Assert.That(board.ItemCount, Is.EqualTo(1));
    }

    [Test]
    public void TryAddItem_WithNoSpace_ReturnsNegativeIndex()
    {
        using var board = new IndexedGridBoard(1, 1);
        var shape = Shapes.ImmutableSingle();

        var (item1, _) = board.TryAddItem(shape);
        var (item2, _) = board.TryAddItem(shape);

        Assert.That(item1.Id, Is.GreaterThanOrEqualTo(0), "First item should be added");
        Assert.That(item2.Id, Is.EqualTo(-1), "Second item should fail");
        Assert.That(board.ItemCount, Is.EqualTo(1));
    }

    [Test]
    public void TryAddItemAt_ValidPosition_ReturnsValidIndex()
    {
        using var board = new IndexedGridBoard(5, 5);
        var shape = Shapes.ImmutableSingle();

        var (item, rotation) = board.TryAddItemAt(shape, 2, 3);

        Assert.That(item.Id, Is.GreaterThanOrEqualTo(0));
        Assert.That(rotation, Is.EqualTo(RotationDegree.None));
        Assert.That(board.ItemCount, Is.EqualTo(1));
    }

    [Test]
    public void TryAddItemAt_InvalidPosition_ReturnsNegativeIndex()
    {
        using var board = new IndexedGridBoard(5, 5);
        var shape = Shapes.ImmutableLine(3);

        var (item, rotation) = board.TryAddItemAt(shape, 4, 0);

        Assert.That(item.Id, Is.EqualTo(-1));
        Assert.That(rotation, Is.EqualTo(RotationDegree.None));
        Assert.That(board.ItemCount, Is.EqualTo(0));
    }

    [Test]
    public void TryAddItemAt_OverlappingPosition_ReturnsNegativeIndex()
    {
        using var board = new IndexedGridBoard(5, 5);
        var shape = Shapes.ImmutableSingle();

        var (item1, _) = board.TryAddItemAt(shape, 2, 2);
        var (item2, _) = board.TryAddItemAt(shape, 2, 2);

        Assert.That(item1.Id, Is.GreaterThanOrEqualTo(0));
        Assert.That(item2.Id, Is.EqualTo(-1));
        Assert.That(board.ItemCount, Is.EqualTo(1));
    }

    [Test]
    public void RemoveItem_ValidIndex_RemovesItem()
    {
        using var board = new IndexedGridBoard(5, 5);
        var shape = Shapes.ImmutableSingle();
        var (item, _) = board.TryAddItemAt(shape, 0, 0);

        Assert.That(board.IsOccupied(0, 0), Is.True, "Cell should be occupied before removal");
        Assert.That(board.ItemCount, Is.EqualTo(1));

        board.RemoveItem(item.Id);

        Assert.That(board.ItemCount, Is.EqualTo(0), "ItemCount decreases after removal");
        Assert.That(board.IsOccupied(0, 0), Is.False, "Cell should be free after removal");
    }

    [Test]
    public void RemoveItem_InvalidIndex_DoesNothing()
    {
        using var board = new IndexedGridBoard(5, 5);
        var shape = Shapes.ImmutableSingle();
        board.TryAddItemAt(shape, 0, 0);

        board.RemoveItem(999);

        Assert.That(board.ItemCount, Is.EqualTo(1));
    }

    [Test]
    public void RemoveItem_ReusesIndex()
    {
        using var board = new IndexedGridBoard(5, 5);
        var shape = Shapes.ImmutableSingle();

        var (item1, _) = board.TryAddItemAt(shape, 0, 0);
        Assert.That(board.ItemCount, Is.EqualTo(1));

        board.RemoveItem(item1.Id);
        Assert.That(board.ItemCount, Is.EqualTo(0), "ItemCount should be 0 after removal");

        var (item2, _) = board.TryAddItemAt(shape, 1, 1);

        Assert.That(item2.Id, Is.EqualTo(item1.Id), "Removed index should be reused");
        Assert.That(board.ItemCount, Is.EqualTo(1), "ItemCount should be 1 after re-adding");
    }

    [Test]
    public void Clear_RemovesAllItems()
    {
        using var board = new IndexedGridBoard(5, 5);
        var shape = Shapes.ImmutableSingle();
        board.TryAddItem(shape);
        board.TryAddItem(shape);

        board.Reset();

        Assert.That(board.ItemCount, Is.EqualTo(0));
    }

    [Test]
    public void Clear_RestoresInitializedGrid()
    {
        using var shape = new GridShape(3, 3);
        shape[0, 0] = true;
        shape[1, 1] = true;
        shape[2, 2] = true;
        // Only cells [0,0], [1,1], [2,2] are available; others are blocked
        using var board = IndexedGridBoard.CreateFromShape(shape);
        var itemShape = Shapes.ImmutableSingle();

        // Add item at an available cell
        board.TryAddItemAt(itemShape, 0, 0);
        Assert.That(board.ItemCount, Is.EqualTo(1));
        Assert.That(board[0, 0], Is.GreaterThanOrEqualTo(0), "Cell should contain item index");

        board.Reset();

        // After clear, item should be removed
        Assert.That(board.ItemCount, Is.EqualTo(0));
        // Available cells should be restored to -1
        Assert.That(board[0, 0], Is.EqualTo(-1), "Available cell should be -1 after clear");
        Assert.That(board[1, 1], Is.EqualTo(-1), "Available cell should be -1 after clear");
        // Blocked cells should remain int.MinValue
        Assert.That(board[1, 0], Is.EqualTo(int.MinValue), "Blocked cell should remain int.MinValue");
        Assert.That(board[0, 1], Is.EqualTo(int.MinValue), "Blocked cell should remain int.MinValue");
    }

    [Test]
    public void Indexer_ReturnsCorrectItemIndex()
    {
        using var board = new IndexedGridBoard(5, 5);
        var shape = Shapes.ImmutableSingle();
        var (item, _) = board.TryAddItemAt(shape, 2, 2);

        var retrievedIndex = board[2, 2];

        Assert.That(retrievedIndex, Is.EqualTo(item.Id));
    }

    [Test]
    public void IsOccupied_ReturnsCorrectValue()
    {
        using var board = new IndexedGridBoard(5, 5);
        var shape = Shapes.ImmutableSingle();
        board.TryAddItemAt(shape, 2, 2);

        Assert.That(board.IsOccupied(2, 2), Is.True);
        Assert.That(board.IsOccupied(0, 0), Is.False);
    }

    [Test]
    public void FreeSpace_ReturnsCorrectCount()
    {
        using var board = new IndexedGridBoard(3, 3);
        var shape = Shapes.ImmutableSingle();

        Assert.That(board.FreeSpace, Is.EqualTo(9));

        board.TryAddItemAt(shape, 0, 0);

        Assert.That(board.FreeSpace, Is.EqualTo(8));
    }

    [Test]
    public void MultipleItems_WithComplexShapes_Work()
    {
        using var board = new IndexedGridBoard(10, 10);

        var line = Shapes.ImmutableLine(3);
        var lShape = Shapes.ImmutableLShape();
        var tShape = Shapes.ImmutableTShape();

        var (item1, _) = board.TryAddItemAt(line, 0, 0);
        var (item2, _) = board.TryAddItemAt(lShape, 0, 1);
        var (item3, _) = board.TryAddItemAt(tShape, 3, 0);

        Assert.That(item1.Id, Is.GreaterThanOrEqualTo(0), "Line should be added");
        Assert.That(item2.Id, Is.GreaterThanOrEqualTo(0), "LShape should be added");
        Assert.That(item3.Id, Is.GreaterThanOrEqualTo(0), "TShape should be added");

        Assert.That(board.ItemCount, Is.EqualTo(3));
    }

    [Test]
    public void Dispose_SecondCallThrowsException()
    {
        var board = new IndexedGridBoard(5, 5);
        board.Dispose();

        Assert.Throws<InvalidOperationException>(() => board.Dispose(),
            "Second dispose should throw because pooled collections were already returned");
    }

    [Test]
    public void AsReadOnly_ReturnsReadOnlyView()
    {
        using var board = new IndexedGridBoard(5, 5);
        var shape = Shapes.ImmutableSingle();
        board.TryAddItemAt(shape, 1, 1);

        var readOnly = board.AsReadOnly();

        Assert.That(readOnly.Width, Is.EqualTo(5));
        Assert.That(readOnly.Height, Is.EqualTo(5));
        Assert.That(readOnly.ItemCount, Is.EqualTo(1));
        Assert.That(readOnly.IsOccupied(1, 1), Is.True);
        var itemData = readOnly.GetItemOnPosition(1, 1);
        Assert.That(itemData.Shape, Is.EqualTo(shape));
    }

    [Test]
    public void ImplicitConversion_ToReadOnly_Works()
    {
        using var board = new IndexedGridBoard(5, 5);
        var shape = Shapes.ImmutableSingle();
        board.TryAddItemAt(shape, 2, 3);

        IndexedGridBoard.ReadOnly readOnly = board;

        Assert.That(readOnly.IsOccupied(2, 3), Is.True);
        var itemData = readOnly.GetItemOnPosition(2, 3);
        Assert.That(itemData.Shape, Is.EqualTo(shape));
    }

    [Test]
    public void TryAddItem_WithRotation_RotatesShape()
    {
        using var board = new IndexedGridBoard(5, 5);
        var shape = Shapes.ImmutableLine(4);

        // Fill the board to block horizontal placement of a 4-unit line, forcing a rotation.
        var single = Shapes.ImmutableSingle();
        for (int y = 0; y < board.Height; y++)
        {
            board.TryAddItemAt(single, 2, y);
        }

        var (item, rotation) = board.TryAddItem(shape);

        Assert.That(item.Id, Is.GreaterThanOrEqualTo(0), "Should find a fit with rotation");
        Assert.That(rotation is RotationDegree.Clockwise90 or RotationDegree.Clockwise270, "A 90 or 270 degree rotation was expected.");
    }

    [Test]
    public void ItemCount_AfterRemoval()
    {
        using var board = new IndexedGridBoard(5, 5);
        var shape = Shapes.ImmutableSingle();

        var (item, _) = board.TryAddItemAt(shape, 1, 1);
        Assert.That(board.ItemCount, Is.EqualTo(1), "ItemCount should be 1 after adding one item");

        board.RemoveItem(item.Id);

        Assert.That(board.ItemCount, Is.EqualTo(0), "ItemCount should be 0 after removal");
    }

    [Test]
    public void ItemCount_AfterAddRemoveAdd()
    {
        using var board = new IndexedGridBoard(5, 5);
        var shape = Shapes.ImmutableSingle();

        // Add first item
        var (item1, _) = board.TryAddItemAt(shape, 0, 0);
        Assert.That(board.ItemCount, Is.EqualTo(1));

        // Remove first item
        board.RemoveItem(item1.Id);
        Assert.That(board.ItemCount, Is.EqualTo(0), "ItemCount should be 0 after removal");

        // Add second item (reuses index)
        var (item2, _) = board.TryAddItemAt(shape, 1, 1);
        Assert.That(item2.Id, Is.EqualTo(item1.Id), "Index should be reused");
        Assert.That(board.ItemCount, Is.EqualTo(1), "ItemCount back to 1");
    }

    [Test]
    public void ItemCount_WithMultipleItems()
    {
        using var board = new IndexedGridBoard(5, 5);
        var shape = Shapes.ImmutableSingle();

        var (item1, _) = board.TryAddItemAt(shape, 0, 0);
        var (item2, _) = board.TryAddItemAt(shape, 1, 0);
        var (item3, _) = board.TryAddItemAt(shape, 2, 0);

        Assert.That(board.ItemCount, Is.EqualTo(3));

        board.RemoveItem(item2.Id);

        Assert.That(board.ItemCount, Is.EqualTo(2), "ItemCount decreased to 2");

        board.RemoveItem(item1.Id);
        board.RemoveItem(item3.Id);

        Assert.That(board.ItemCount, Is.EqualTo(0), "ItemCount is 0");
    }

    [Test]
    public void ItemCount_AfterClear()
    {
        using var board = new IndexedGridBoard(5, 5);
        var shape = Shapes.ImmutableSingle();

        board.TryAddItemAt(shape, 0, 0);
        board.TryAddItemAt(shape, 1, 0);

        Assert.That(board.ItemCount, Is.EqualTo(2));

        board.Reset();

        Assert.That(board.ItemCount, Is.EqualTo(0), "ItemCount reset to 0 after clear");
    }

    [Test]
    public void GetHashCode_ThrowsNotSupportedException()
    {
        using var board = new IndexedGridBoard(3, 3);

        Assert.Throws<NotSupportedException>(() => board.GetHashCode());
    }

    [Test]
    public void Equals_Object_ThrowsNotSupportedException()
    {
        using var board = new IndexedGridBoard(3, 3);

        Assert.Throws<NotSupportedException>(() => board.Equals((object)board));
    }

    [Test]
    public void Equals_IndexedGridBoard_ThrowsNotSupportedException()
    {
        using var board1 = new IndexedGridBoard(3, 3);
        using var board2 = new IndexedGridBoard(3, 3);

        Assert.Throws<NotSupportedException>(() => board1.Equals(board2));
    }

    [Test]
    public void ReadOnly_GetHashCode_ThrowsNotSupportedException()
    {
        using var board = new IndexedGridBoard(3, 3);
        var readOnly = board.AsReadOnly();

        Assert.Throws<NotSupportedException>(() => readOnly.GetHashCode());
    }

    [Test]
    public void CreateFromShape_WithGridShape_UnoccupiedCellsMarkedAsMinValue()
    {
        // Create an L-shaped container
        using var shape = new GridShape(3, 3);
        shape[0, 0] = true;
        shape[1, 0] = true;
        shape[0, 1] = true;
        // [2,0], [1,1], [2,1], [0,2], [1,2], [2,2] are unoccupied

        using var board = IndexedGridBoard.CreateFromShape(shape);

        // Occupied cells should have -1 (empty value)
        Assert.That(board[0, 0], Is.EqualTo(-1), "Occupied container cell should have -1");
        Assert.That(board[1, 0], Is.EqualTo(-1), "Occupied container cell should have -1");
        Assert.That(board[0, 1], Is.EqualTo(-1), "Occupied container cell should have -1");

        // Unoccupied cells should have int.MinValue (unavailable)
        Assert.That(board[2, 0], Is.EqualTo(int.MinValue), "Unoccupied container cell should have int.MinValue");
        Assert.That(board[1, 1], Is.EqualTo(int.MinValue), "Unoccupied container cell should have int.MinValue");
        Assert.That(board[2, 2], Is.EqualTo(int.MinValue), "Unoccupied container cell should have int.MinValue");

        // IsOccupied should return true for cells with int.MinValue (they're permanently blocked)
        Assert.That(board.IsOccupied(2, 0), Is.True, "Unoccupied container cells should be marked as occupied");
        Assert.That(board.IsOccupied(1, 1), Is.True, "Unoccupied container cells should be marked as occupied");
    }

    [Test]
    public void RemoveItem_RestoresCorrectEmptyValue()
    {
        using var board = new IndexedGridBoard(5, 5);

        var itemShape = Shapes.ImmutableSingle();

        // Add item at position (2, 2)
        var (item, _) = board.TryAddItemAt(itemShape, 2, 2);
        Assert.That(item.Id, Is.GreaterThanOrEqualTo(0), "Item should be added successfully");
        Assert.That(board.IsOccupied(2, 2), Is.True, "Cell should be occupied after adding item");
        Assert.That(board[2, 2], Is.EqualTo(item.Id), "Cell should contain the item index");

        // Remove the item
        board.RemoveItem(item.Id);

        // Cell should be restored to -1 (empty value)
        Assert.That(board.IsOccupied(2, 2), Is.False, "Cell should not be occupied after removal");
        Assert.That(board[2, 2], Is.EqualTo(-1), "Cell should contain -1 after removal");
    }

    [Test]
    public void FreeSpace_CountIsCorrectAfterRemoval()
    {
        using var board = new IndexedGridBoard(5, 5);

        var itemShape = Shapes.ImmutableSingle();

        int initialFreeSpace = board.FreeSpace;
        Assert.That(initialFreeSpace, Is.EqualTo(25), "All cells should be free initially");

        // Add an item
        var (item, _) = board.TryAddItemAt(itemShape, 1, 1);
        Assert.That(board.FreeSpace, Is.EqualTo(24), "Free space should decrease by 1");

        // Remove the item
        board.RemoveItem(item.Id);

        Assert.That(board.FreeSpace, Is.EqualTo(25), "Free space should be restored after removal");
    }

    [Test]
    public void CanReuseSlotAfterRemoval()
    {
        using var board = new IndexedGridBoard(5, 5);

        var itemShape = Shapes.ImmutableSingle();

        // Add first item
        var (item1, _) = board.TryAddItemAt(itemShape, 2, 2);
        Assert.That(item1.Id, Is.GreaterThanOrEqualTo(0));

        // Remove first item
        board.RemoveItem(item1.Id);

        // Should be able to add second item at the same position
        var (item2, _) = board.TryAddItemAt(itemShape, 2, 2);
        Assert.That(item2.Id, Is.GreaterThanOrEqualTo(0), "Should be able to reuse the slot after removal");
        var itemData = board.GetItemById(item2.Id);
        Assert.That(itemData.Id, Is.EqualTo(item2.Id));
    }

    [Test]
    public void ReadOnly_Equals_ThrowsNotSupportedException()
    {
        using var board1 = new IndexedGridBoard(3, 3);
        using var board2 = new IndexedGridBoard(3, 3);
        var readOnly1 = board1.AsReadOnly();
        var readOnly2 = board2.AsReadOnly();

        Assert.Throws<NotSupportedException>(() => readOnly1.Equals(readOnly2));
    }

    [Test]
    public void Enumerator_IteratesValidItemsOnly()
    {
        using var board = new IndexedGridBoard(5, 5);
        var shape = Shapes.ImmutableSingle();

        var (item1, _) = board.TryAddItemAt(shape, 0, 0);
        var (item2, _) = board.TryAddItemAt(shape, 1, 0);
        var (item3, _) = board.TryAddItemAt(shape, 2, 0);

        // Remove middle item
        board.RemoveItem(item2.Id);

        var enumeratedItems = new List<BoardItemData>();
        foreach (var item in board)
        {
            enumeratedItems.Add(item);
        }

        Assert.That(enumeratedItems.Count, Is.EqualTo(2), "Should only enumerate active items");

        var indices = enumeratedItems.Select(x => x.Id).ToList();

        Assert.That(indices, Does.Contain(item1.Id));
        Assert.That(indices, Does.Contain(item3.Id));
    }

    [Test]
    public void Enumerator_EmptyBoard_NoIterations()
    {
        using var board = new IndexedGridBoard(5, 5);

        var count = 0;
        foreach (var _ in board)
        {
            count++;
        }

        Assert.That(count, Is.EqualTo(0));
    }

    [Test]
    public void Enumerator_AllItemsRemoved_NoIterations()
    {
        using var board = new IndexedGridBoard(5, 5);
        var shape = Shapes.ImmutableSingle();

        var (item1, _) = board.TryAddItemAt(shape, 0, 0);
        var (item2, _) = board.TryAddItemAt(shape, 1, 0);

        board.RemoveItem(item1.Id);
        board.RemoveItem(item2.Id);

        var count = 0;
        foreach (var _ in board)
        {
            count++;
        }

        Assert.That(count, Is.EqualTo(0));
    }

    [Test]
    public void Enumerator_ReadOnly_IteratesValidItemsOnly()
    {
        using var board = new IndexedGridBoard(5, 5);
        var shape = Shapes.ImmutableSingle();

        var (item1, _) = board.TryAddItemAt(shape, 0, 0);
        var (item2, _) = board.TryAddItemAt(shape, 1, 0);
        board.RemoveItem(item2.Id);

        var readOnly = board.AsReadOnly();
        var enumeratedItems = new List<BoardItemData>();
        foreach (var item in readOnly)
        {
            enumeratedItems.Add(item);
        }

        Assert.That(enumeratedItems.Count, Is.EqualTo(1));
        Assert.That(enumeratedItems[0].Id, Is.EqualTo(item1.Id));
    }

    [Test]
    public void Enumerator_WithComplexPattern_IteratesCorrectly()
    {
        using var board = new IndexedGridBoard(10, 10);
        var shape = Shapes.ImmutableSingle();

        var items = new List<BoardItemData>();
        for (int i = 0; i < 5; i++)
        {
            var (item, _) = board.TryAddItemAt(shape, i, 0);
            items.Add(item);
        }

        // Remove items at indices 1 and 3
        board.RemoveItem(items[1].Id);
        board.RemoveItem(items[3].Id);

        var enumeratedIndices = new List<int>();
        foreach (var itemData in board)
        {
            enumeratedIndices.Add(itemData.Id);
        }

        Assert.That(enumeratedIndices.Count, Is.EqualTo(3));
        Assert.That(enumeratedIndices, Does.Contain(items[0].Id));
        Assert.That(enumeratedIndices, Does.Contain(items[2].Id));
        Assert.That(enumeratedIndices, Does.Contain(items[4].Id));
        Assert.That(enumeratedIndices, Has.No.Member(items[1].Id));
        Assert.That(enumeratedIndices, Has.No.Member(items[3].Id));
    }

    [Test]
    public void IndexReuse_ReusesFreedIndices()
    {
        using var board = new IndexedGridBoard(10, 10);
        var shape = Shapes.ImmutableSingle();

        // Add 5 items
        var (item0, _) = board.TryAddItemAt(shape, 0, 0);
        var (item1, _) = board.TryAddItemAt(shape, 1, 0);
        var (item2, _) = board.TryAddItemAt(shape, 2, 0);
        var (item3, _) = board.TryAddItemAt(shape, 3, 0);
        var (item4, _) = board.TryAddItemAt(shape, 4, 0);

        Assert.That(item0.Id, Is.EqualTo(0));
        Assert.That(item1.Id, Is.EqualTo(1));
        Assert.That(item2.Id, Is.EqualTo(2));
        Assert.That(item3.Id, Is.EqualTo(3));
        Assert.That(item4.Id, Is.EqualTo(4));

        // Remove indices 1, 3, 2 (in that order)
        board.RemoveItem(item1.Id);
        board.RemoveItem(item3.Id);
        board.RemoveItem(item2.Id);

        var reusedIndices = new HashSet<int>();

        // Add new items - should reuse freed indices (order unspecified)
        var (newItem1, _) = board.TryAddItemAt(shape, 5, 0);
        var (newItem2, _) = board.TryAddItemAt(shape, 6, 0);
        var (newItem3, _) = board.TryAddItemAt(shape, 7, 0);
        reusedIndices.Add(newItem1.Id);
        reusedIndices.Add(newItem2.Id);
        reusedIndices.Add(newItem3.Id);

        Assert.That(reusedIndices, Is.EquivalentTo(new[] { 1, 2, 3 }), "Should reuse all freed indices");

        // Next item should get a new index
        var (newItem, _) = board.TryAddItemAt(shape, 8, 0);
        Assert.That(newItem.Id, Is.EqualTo(5), "Should allocate new index after all free indices used");
    }

    [Test]
    public void GetItemById_ReturnsCorrectItemData()
    {
        using var board = new IndexedGridBoard(10, 10);
        var shape = Shapes.ImmutableSingle();

        var (item, _) = board.TryAddItemAt(shape, 3, 4);

        var itemData = board.GetItemById(item.Id);

        Assert.That(itemData.Id, Is.EqualTo(item.Id));
        Assert.That(itemData.Shape, Is.EqualTo(shape));
        Assert.That(itemData.X, Is.EqualTo(3));
        Assert.That(itemData.Y, Is.EqualTo(4));
    }

    [Test]
    public void GetItemById_WithMultiCellShape_ReturnsCorrectData()
    {
        using var board = new IndexedGridBoard(10, 10);
        var shape = Shapes.ImmutableLine(3);

        var (item, _) = board.TryAddItemAt(shape, 2, 5);

        var itemData = board.GetItemById(item.Id);

        Assert.That(itemData.Id, Is.EqualTo(item.Id));
        Assert.That(itemData.Shape, Is.EqualTo(shape));
        Assert.That(itemData.X, Is.EqualTo(2));
        Assert.That(itemData.Y, Is.EqualTo(5));
    }

    [Test]
    public void GetItemOnPosition_ReturnsCorrectItemData()
    {
        using var board = new IndexedGridBoard(10, 10);
        var shape = Shapes.ImmutableSingle();

        var (item, _) = board.TryAddItemAt(shape, 6, 7);

        var itemData = board.GetItemOnPosition(6, 7);

        Assert.That(itemData.Id, Is.EqualTo(item.Id));
        Assert.That(itemData.Shape, Is.EqualTo(shape));
        Assert.That(itemData.X, Is.EqualTo(6));
        Assert.That(itemData.Y, Is.EqualTo(7));
    }

    [Test]
    public void GetItemOnPosition_WithMultiCellShape_ReturnsCorrectDataFromAnyCell()
    {
        using var board = new IndexedGridBoard(10, 10);
        var shape = Shapes.ImmutableLine(4); // 4-cell horizontal line

        var (item, _) = board.TryAddItemAt(shape, 1, 3);

        // Test getting item from different cells of the same shape
        var result1 = board.GetItemOnPosition(1, 3);
        var result2 = board.GetItemOnPosition(2, 3);
        var result3 = board.GetItemOnPosition(3, 3);
        var result4 = board.GetItemOnPosition(4, 3);

        // All should return the same shape
        Assert.That(result1.Shape, Is.EqualTo(shape));
        Assert.That(result2.Shape, Is.EqualTo(shape));
        Assert.That(result3.Shape, Is.EqualTo(shape));
        Assert.That(result4.Shape, Is.EqualTo(shape));

        // All should return the same ID
        Assert.That(result1.Id, Is.EqualTo(item.Id));
        Assert.That(result2.Id, Is.EqualTo(item.Id));
        Assert.That(result3.Id, Is.EqualTo(item.Id));
        Assert.That(result4.Id, Is.EqualTo(item.Id));

        // All should return the same position (origin of the shape)
        Assert.That(result1.X, Is.EqualTo(1));
        Assert.That(result2.X, Is.EqualTo(1));
        Assert.That(result3.X, Is.EqualTo(1));
        Assert.That(result4.X, Is.EqualTo(1));
    }

    [Test]
    public void GetItemById_AfterRemoval_ReturnsUpdatedData()
    {
        using var board = new IndexedGridBoard(10, 10);
        var shape = Shapes.ImmutableSingle();

        var (item1, _) = board.TryAddItemAt(shape, 2, 2);
        board.RemoveItem(item1.Id);

        // Reuse the same index
        var (item2, _) = board.TryAddItemAt(shape, 5, 5);

        var itemData = board.GetItemById(item2.Id);

        Assert.That(itemData.Shape, Is.EqualTo(shape));
        Assert.That(itemData.X, Is.EqualTo(5));
        Assert.That(itemData.Y, Is.EqualTo(5));
    }

    [Test]
    public void ReadOnly_GetItemById_ReturnsCorrectData()
    {
        using var board = new IndexedGridBoard(10, 10);
        var shape = Shapes.ImmutableSingle();

        var (item, _) = board.TryAddItemAt(shape, 4, 4);
        var readOnly = board.AsReadOnly();

        var itemData = readOnly.GetItemById(item.Id);

        Assert.That(itemData.Id, Is.EqualTo(item.Id));
        Assert.That(itemData.Shape, Is.EqualTo(shape));
        Assert.That(itemData.X, Is.EqualTo(4));
        Assert.That(itemData.Y, Is.EqualTo(4));
    }

    [Test]
    public void ReadOnly_GetItemOnPosition_ReturnsCorrectData()
    {
        using var board = new IndexedGridBoard(10, 10);
        var shape = Shapes.ImmutableSingle();

        board.TryAddItemAt(shape, 8, 9);
        var readOnly = board.AsReadOnly();

        var itemData = readOnly.GetItemOnPosition(8, 9);

        Assert.That(itemData.Shape, Is.EqualTo(shape));
        Assert.That(itemData.X, Is.EqualTo(8));
        Assert.That(itemData.Y, Is.EqualTo(9));
    }
}
