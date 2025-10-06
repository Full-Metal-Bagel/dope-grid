using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace DopeGrid.Tests;

[TestFixture]
public class IndexedGridBoardTests
{
    private record struct ItemData(string Name, int Value);

    [Test]
    public void Constructor_WithDimensions_CreatesBoard()
    {
        using var board = new IndexedGridBoard<ItemData>(5, 5);

        Assert.That(board.Width, Is.EqualTo(5));
        Assert.That(board.Height, Is.EqualTo(5));
        Assert.That(board.ItemCount, Is.EqualTo(0));
    }

    [Test]
    public void CreateFromShape_WithGridShape_CreatesBoard()
    {
        using var shape = new GridShape(3, 3);
        shape.FillAll(true);
        using var board = IndexedGridBoard<ItemData>.CreateFromShape(shape);

        Assert.That(board.Width, Is.EqualTo(3));
        Assert.That(board.Height, Is.EqualTo(3));
    }

    [Test]
    public void CreateFromShape_WithEmptyShape_CreatesEmptyBoard()
    {
        using var shape = new GridShape(0, 0);
        using var board = IndexedGridBoard<ItemData>.CreateFromShape(shape);

        Assert.That(board.Width, Is.EqualTo(0));
        Assert.That(board.Height, Is.EqualTo(0));
    }

    [Test]
    public void TryAddItem_WithValidItem_ReturnsIndexAndRotation()
    {
        using var board = new IndexedGridBoard<ItemData>(5, 5);
        var item = new ItemData("Sword", 100);
        var shape = Shapes.ImmutableSingle();

        var (index, rotation) = board.TryAddItem(item, shape);

        Assert.That(index, Is.GreaterThanOrEqualTo(0));
        Assert.That(board.ItemCount, Is.EqualTo(1));
    }

    [Test]
    public void TryAddItem_WithNoSpace_ReturnsNegativeIndex()
    {
        using var board = new IndexedGridBoard<ItemData>(1, 1);
        var item1 = new ItemData("Item1", 1);
        var item2 = new ItemData("Item2", 2);
        var shape = Shapes.ImmutableSingle();

        var (index1, _) = board.TryAddItem(item1, shape);
        var (index2, _) = board.TryAddItem(item2, shape);

        Assert.That(index1, Is.GreaterThanOrEqualTo(0), "First item should be added");
        Assert.That(index2, Is.EqualTo(-1), "Second item should fail");
        Assert.That(board.ItemCount, Is.EqualTo(1));
    }

    [Test]
    public void TryAddItemAt_ValidPosition_ReturnsValidIndex()
    {
        using var board = new IndexedGridBoard<ItemData>(5, 5);
        var item = new ItemData("Potion", 50);
        var shape = Shapes.ImmutableSingle();

        var index = board.TryAddItemAt(item, shape, 2, 3);

        Assert.That(index, Is.GreaterThanOrEqualTo(0));
        Assert.That(board.ItemCount, Is.EqualTo(1));
    }

    [Test]
    public void TryAddItemAt_InvalidPosition_ReturnsNegativeIndex()
    {
        using var board = new IndexedGridBoard<ItemData>(5, 5);
        var item = new ItemData("Spear", 75);
        var shape = Shapes.ImmutableLine(3);

        var index = board.TryAddItemAt(item, shape, 4, 0);

        Assert.That(index, Is.EqualTo(-1));
        Assert.That(board.ItemCount, Is.EqualTo(0));
    }

    [Test]
    public void TryAddItemAt_OverlappingPosition_ReturnsNegativeIndex()
    {
        using var board = new IndexedGridBoard<ItemData>(5, 5);
        var item1 = new ItemData("Item1", 1);
        var item2 = new ItemData("Item2", 2);
        var shape = Shapes.ImmutableSingle();

        var index1 = board.TryAddItemAt(item1, shape, 2, 2);
        var index2 = board.TryAddItemAt(item2, shape, 2, 2);

        Assert.That(index1, Is.GreaterThanOrEqualTo(0));
        Assert.That(index2, Is.EqualTo(-1));
        Assert.That(board.ItemCount, Is.EqualTo(1));
    }

    [Test]
    public void RemoveItem_ValidIndex_RemovesItem()
    {
        using var board = new IndexedGridBoard<ItemData>(5, 5);
        var item = new ItemData("Shield", 80);
        var shape = Shapes.ImmutableSingle();
        var index = board.TryAddItemAt(item, shape, 0, 0);

        Assert.That(board.IsOccupied(0, 0), Is.True, "Cell should be occupied before removal");
        Assert.That(board.ItemCount, Is.EqualTo(1));

        board.RemoveItem(index);

        Assert.That(board.ItemCount, Is.EqualTo(0), "ItemCount decreases after removal");
        Assert.That(board.IsOccupied(0, 0), Is.False, "Cell should be free after removal");
    }

    [Test]
    public void RemoveItem_InvalidIndex_DoesNothing()
    {
        using var board = new IndexedGridBoard<ItemData>(5, 5);
        var item = new ItemData("Helmet", 60);
        var shape = Shapes.ImmutableSingle();
        board.TryAddItemAt(item, shape, 0, 0);

        board.RemoveItem(999);

        Assert.That(board.ItemCount, Is.EqualTo(1));
    }

    [Test]
    public void RemoveItem_ReusesIndex()
    {
        using var board = new IndexedGridBoard<ItemData>(5, 5);
        var item1 = new ItemData("Item1", 1);
        var item2 = new ItemData("Item2", 2);
        var shape = Shapes.ImmutableSingle();

        var index1 = board.TryAddItemAt(item1, shape, 0, 0);
        Assert.That(board.ItemCount, Is.EqualTo(1));

        board.RemoveItem(index1);
        Assert.That(board.ItemCount, Is.EqualTo(0), "ItemCount should be 0 after removal");

        var index2 = board.TryAddItemAt(item2, shape, 1, 1);

        Assert.That(index2, Is.EqualTo(index1), "Removed index should be reused");
        Assert.That(board.ItemCount, Is.EqualTo(1), "ItemCount should be 1 after re-adding");
    }

    [Test]
    public void Clear_RemovesAllItems()
    {
        using var board = new IndexedGridBoard<ItemData>(5, 5);
        var item = new ItemData("Axe", 90);
        var shape = Shapes.ImmutableSingle();
        board.TryAddItem(item, shape);
        board.TryAddItem(item, shape);

        board.Clear();

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
        using var board = IndexedGridBoard<ItemData>.CreateFromShape(shape);
        var item = new ItemData("Bow", 70);
        var itemShape = Shapes.ImmutableSingle();

        // Add item at an available cell
        board.TryAddItemAt(item, itemShape, 0, 0);
        Assert.That(board.ItemCount, Is.EqualTo(1));
        Assert.That(board[0, 0], Is.GreaterThanOrEqualTo(0), "Cell should contain item index");

        board.Clear();

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
        using var board = new IndexedGridBoard<ItemData>(5, 5);
        var item = new ItemData("Ring", 150);
        var shape = Shapes.ImmutableSingle();
        var itemIndex = board.TryAddItemAt(item, shape, 2, 2);

        var retrievedIndex = board[2, 2];

        Assert.That(retrievedIndex, Is.EqualTo(itemIndex));
    }

    [Test]
    public void IsOccupied_ReturnsCorrectValue()
    {
        using var board = new IndexedGridBoard<ItemData>(5, 5);
        var item = new ItemData("Amulet", 120);
        var shape = Shapes.ImmutableSingle();
        board.TryAddItemAt(item, shape, 2, 2);

        Assert.That(board.IsOccupied(2, 2), Is.True);
        Assert.That(board.IsOccupied(0, 0), Is.False);
    }

    [Test]
    public void FreeSpace_ReturnsCorrectCount()
    {
        using var board = new IndexedGridBoard<ItemData>(3, 3);
        var item = new ItemData("Coin", 1);
        var shape = Shapes.ImmutableSingle();

        Assert.That(board.FreeSpace, Is.EqualTo(9));

        board.TryAddItemAt(item, shape, 0, 0);

        Assert.That(board.FreeSpace, Is.EqualTo(8));
    }

    [Test]
    public void MultipleItems_WithComplexShapes_Work()
    {
        using var board = new IndexedGridBoard<ItemData>(10, 10);
        var item1 = new ItemData("Sword", 100);
        var item2 = new ItemData("Shield", 80);
        var item3 = new ItemData("Helmet", 60);

        var line = Shapes.ImmutableLine(3);
        var lShape = Shapes.ImmutableLShape();
        var tShape = Shapes.ImmutableTShape();

        var index1 = board.TryAddItemAt(item1, line, 0, 0);
        var index2 = board.TryAddItemAt(item2, lShape, 0, 1);
        var index3 = board.TryAddItemAt(item3, tShape, 3, 0);

        Assert.That(index1, Is.GreaterThanOrEqualTo(0), "Line should be added");
        Assert.That(index2, Is.GreaterThanOrEqualTo(0), "LShape should be added");
        Assert.That(index3, Is.GreaterThanOrEqualTo(0), "TShape should be added");

        Assert.That(board.ItemCount, Is.EqualTo(3));
    }

    [Test]
    public void Dispose_SecondCallThrowsException()
    {
        var board = new IndexedGridBoard<ItemData>(5, 5);
        board.Dispose();

        Assert.Throws<InvalidOperationException>(() => board.Dispose(),
            "Second dispose should throw because pooled collections were already returned");
    }

    [Test]
    public void AsReadOnly_ReturnsReadOnlyView()
    {
        using var board = new IndexedGridBoard<ItemData>(5, 5);
        var item = new ItemData("Gem", 200);
        var shape = Shapes.ImmutableSingle();
        board.TryAddItemAt(item, shape, 1, 1);

        var readOnly = board.AsReadOnly();

        Assert.That(readOnly.Width, Is.EqualTo(5));
        Assert.That(readOnly.Height, Is.EqualTo(5));
        Assert.That(readOnly.ItemCount, Is.EqualTo(1));
        Assert.That(readOnly.IsOccupied(1, 1), Is.True);
        var (_, data, _, _, _) = readOnly.GetItemOnPosition(1, 1);
        Assert.That(data, Is.EqualTo(item));
    }

    [Test]
    public void ImplicitConversion_ToReadOnly_Works()
    {
        using var board = new IndexedGridBoard<ItemData>(5, 5);
        var item = new ItemData("Scroll", 30);
        var shape = Shapes.ImmutableSingle();
        board.TryAddItemAt(item, shape, 2, 3);

        IndexedGridBoard<ItemData>.ReadOnly readOnly = board;

        Assert.That(readOnly.IsOccupied(2, 3), Is.True);
        var (_, data, _, _, _) = readOnly.GetItemOnPosition(2, 3);
        Assert.That(data, Is.EqualTo(item));
    }

    [Test]
    public void TryAddItem_WithRotation_RotatesShape()
    {
        using var board = new IndexedGridBoard<ItemData>(5, 5);
        var item = new ItemData("LongSword", 110);
        var shape = Shapes.ImmutableLine(4);

        // Fill the board to block horizontal placement of a 4-unit line, forcing a rotation.
        var filler = new ItemData("Filler", 1);
        var single = Shapes.ImmutableSingle();
        for (int y = 0; y < board.Height; y++)
        {
            board.TryAddItemAt(filler, single, 2, y);
        }

        var (index, rotation) = board.TryAddItem(item, shape);

        Assert.That(index, Is.GreaterThanOrEqualTo(0), "Should find a fit with rotation");
        Assert.That(rotation is RotationDegree.Clockwise90 or RotationDegree.Clockwise270, "A 90 or 270 degree rotation was expected.");
    }

    [Test]
    public void ItemCount_AfterRemoval()
    {
        using var board = new IndexedGridBoard<ItemData>(5, 5);
        var item = new ItemData("Staff", 95);
        var shape = Shapes.ImmutableSingle();

        var index = board.TryAddItemAt(item, shape, 1, 1);
        Assert.That(board.ItemCount, Is.EqualTo(1), "ItemCount should be 1 after adding one item");

        board.RemoveItem(index);

        Assert.That(board.ItemCount, Is.EqualTo(0), "ItemCount should be 0 after removal");
    }

    [Test]
    public void ItemCount_AfterAddRemoveAdd()
    {
        using var board = new IndexedGridBoard<ItemData>(5, 5);
        var item1 = new ItemData("Sword", 100);
        var item2 = new ItemData("Shield", 80);
        var shape = Shapes.ImmutableSingle();

        // Add first item
        var index1 = board.TryAddItemAt(item1, shape, 0, 0);
        Assert.That(board.ItemCount, Is.EqualTo(1));

        // Remove first item
        board.RemoveItem(index1);
        Assert.That(board.ItemCount, Is.EqualTo(0), "ItemCount should be 0 after removal");

        // Add second item (reuses index)
        var index2 = board.TryAddItemAt(item2, shape, 1, 1);
        Assert.That(index2, Is.EqualTo(index1), "Index should be reused");
        Assert.That(board.ItemCount, Is.EqualTo(1), "ItemCount back to 1");
    }

    [Test]
    public void ItemCount_WithMultipleItems()
    {
        using var board = new IndexedGridBoard<ItemData>(5, 5);
        var item = new ItemData("Item", 1);
        var shape = Shapes.ImmutableSingle();

        var index1 = board.TryAddItemAt(item, shape, 0, 0);
        var index2 = board.TryAddItemAt(item, shape, 1, 0);
        var index3 = board.TryAddItemAt(item, shape, 2, 0);

        Assert.That(board.ItemCount, Is.EqualTo(3));

        board.RemoveItem(index2);

        Assert.That(board.ItemCount, Is.EqualTo(2), "ItemCount decreased to 2");

        board.RemoveItem(index1);
        board.RemoveItem(index3);

        Assert.That(board.ItemCount, Is.EqualTo(0), "ItemCount is 0");
    }

    [Test]
    public void ItemCount_AfterClear()
    {
        using var board = new IndexedGridBoard<ItemData>(5, 5);
        var item = new ItemData("Potion", 50);
        var shape = Shapes.ImmutableSingle();

        board.TryAddItemAt(item, shape, 0, 0);
        board.TryAddItemAt(item, shape, 1, 0);

        Assert.That(board.ItemCount, Is.EqualTo(2));

        board.Clear();

        Assert.That(board.ItemCount, Is.EqualTo(0), "ItemCount reset to 0 after clear");
    }

    [Test]
    public void GetHashCode_ThrowsNotSupportedException()
    {
        using var board = new IndexedGridBoard<ItemData>(3, 3);

        Assert.Throws<NotSupportedException>(() => board.GetHashCode());
    }

    [Test]
    public void Equals_Object_ThrowsNotSupportedException()
    {
        using var board = new IndexedGridBoard<ItemData>(3, 3);

        Assert.Throws<NotSupportedException>(() => board.Equals((object)board));
    }

    [Test]
    public void Equals_IndexedGridBoard_ThrowsNotSupportedException()
    {
        using var board1 = new IndexedGridBoard<ItemData>(3, 3);
        using var board2 = new IndexedGridBoard<ItemData>(3, 3);

        Assert.Throws<NotSupportedException>(() => board1.Equals(board2));
    }

    [Test]
    public void ReadOnly_GetHashCode_ThrowsNotSupportedException()
    {
        using var board = new IndexedGridBoard<ItemData>(3, 3);
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

        using var board = IndexedGridBoard<ItemData>.CreateFromShape(shape);

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
        using var board = new IndexedGridBoard<ItemData>(5, 5);

        var item = new ItemData("Sword", 100);
        var itemShape = Shapes.ImmutableSingle();

        // Add item at position (2, 2)
        var index = board.TryAddItemAt(item, itemShape, 2, 2);
        Assert.That(index, Is.GreaterThanOrEqualTo(0), "Item should be added successfully");
        Assert.That(board.IsOccupied(2, 2), Is.True, "Cell should be occupied after adding item");
        Assert.That(board[2, 2], Is.EqualTo(index), "Cell should contain the item index");

        // Remove the item
        board.RemoveItem(index);

        // Cell should be restored to -1 (empty value)
        Assert.That(board.IsOccupied(2, 2), Is.False, "Cell should not be occupied after removal");
        Assert.That(board[2, 2], Is.EqualTo(-1), "Cell should contain -1 after removal");
    }

    [Test]
    public void FreeSpace_CountIsCorrectAfterRemoval()
    {
        using var board = new IndexedGridBoard<ItemData>(5, 5);

        var item = new ItemData("Potion", 50);
        var itemShape = Shapes.ImmutableSingle();

        int initialFreeSpace = board.FreeSpace;
        Assert.That(initialFreeSpace, Is.EqualTo(25), "All cells should be free initially");

        // Add an item
        var index = board.TryAddItemAt(item, itemShape, 1, 1);
        Assert.That(board.FreeSpace, Is.EqualTo(24), "Free space should decrease by 1");

        // Remove the item
        board.RemoveItem(index);

        Assert.That(board.FreeSpace, Is.EqualTo(25), "Free space should be restored after removal");
    }

    [Test]
    public void CanReuseSlotAfterRemoval()
    {
        using var board = new IndexedGridBoard<ItemData>(5, 5);

        var item1 = new ItemData("Staff", 75);
        var item2 = new ItemData("Wand", 50);
        var itemShape = Shapes.ImmutableSingle();

        // Add first item
        var index1 = board.TryAddItemAt(item1, itemShape, 2, 2);
        Assert.That(index1, Is.GreaterThanOrEqualTo(0));

        // Remove first item
        board.RemoveItem(index1);

        // Should be able to add second item at the same position
        var index2 = board.TryAddItemAt(item2, itemShape, 2, 2);
        Assert.That(index2, Is.GreaterThanOrEqualTo(0), "Should be able to reuse the slot after removal");
        var (_, data, _, _, _) = board.GetItemById(index2);
        Assert.That(data.Name, Is.EqualTo("Wand"));
    }

    [Test]
    public void ReadOnly_Equals_ThrowsNotSupportedException()
    {
        using var board1 = new IndexedGridBoard<ItemData>(3, 3);
        using var board2 = new IndexedGridBoard<ItemData>(3, 3);
        var readOnly1 = board1.AsReadOnly();
        var readOnly2 = board2.AsReadOnly();

        Assert.Throws<NotSupportedException>(() => readOnly1.Equals(readOnly2));
    }

    [Test]
    public void Enumerator_IteratesValidItemsOnly()
    {
        using var board = new IndexedGridBoard<ItemData>(5, 5);
        var item1 = new ItemData("Sword", 100);
        var item2 = new ItemData("Shield", 80);
        var item3 = new ItemData("Potion", 50);
        var shape = Shapes.ImmutableSingle();

        var index1 = board.TryAddItemAt(item1, shape, 0, 0);
        var index2 = board.TryAddItemAt(item2, shape, 1, 0);
        var index3 = board.TryAddItemAt(item3, shape, 2, 0);

        // Remove middle item
        board.RemoveItem(index2);

        var enumeratedItems = new List<IndexedGridBoard<ItemData>.ItemData>();
        foreach (var item in board)
        {
            enumeratedItems.Add(item);
        }

        Assert.That(enumeratedItems.Count, Is.EqualTo(2), "Should only enumerate active items");

        var indices = enumeratedItems.Select(x => x.Id).ToList();
        var data = enumeratedItems.Select(x => x.Data).ToList();

        Assert.That(indices, Does.Contain(index1));
        Assert.That(indices, Does.Contain(index3));
        Assert.That(data, Does.Contain(item1));
        Assert.That(data, Does.Contain(item3));
    }

    [Test]
    public void Enumerator_EmptyBoard_NoIterations()
    {
        using var board = new IndexedGridBoard<ItemData>(5, 5);

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
        using var board = new IndexedGridBoard<ItemData>(5, 5);
        var item = new ItemData("Item", 1);
        var shape = Shapes.ImmutableSingle();

        var index1 = board.TryAddItemAt(item, shape, 0, 0);
        var index2 = board.TryAddItemAt(item, shape, 1, 0);

        board.RemoveItem(index1);
        board.RemoveItem(index2);

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
        using var board = new IndexedGridBoard<ItemData>(5, 5);
        var item1 = new ItemData("Ring", 150);
        var item2 = new ItemData("Amulet", 120);
        var shape = Shapes.ImmutableSingle();

        board.TryAddItemAt(item1, shape, 0, 0);
        var index2 = board.TryAddItemAt(item2, shape, 1, 0);
        board.RemoveItem(index2);

        var readOnly = board.AsReadOnly();
        var enumeratedItems = new List<IndexedGridBoard<ItemData>.ItemData>();
        foreach (var item in readOnly)
        {
            enumeratedItems.Add(item);
        }

        Assert.That(enumeratedItems.Count, Is.EqualTo(1));
        Assert.That(enumeratedItems[0].Data, Is.EqualTo(item1));
    }

    [Test]
    public void Enumerator_WithComplexPattern_IteratesCorrectly()
    {
        using var board = new IndexedGridBoard<ItemData>(10, 10);
        var shape = Shapes.ImmutableSingle();

        var indices = new List<int>();
        for (int i = 0; i < 5; i++)
        {
            var item = new ItemData($"Item{i}", i);
            indices.Add(board.TryAddItemAt(item, shape, i, 0));
        }

        // Remove items at indices 1 and 3
        board.RemoveItem(indices[1]);
        board.RemoveItem(indices[3]);

        var enumeratedIndices = new List<int>();
        foreach (var (index, _, _, _, _) in board)
        {
            enumeratedIndices.Add(index);
        }

        Assert.That(enumeratedIndices.Count, Is.EqualTo(3));
        Assert.That(enumeratedIndices, Does.Contain(indices[0]));
        Assert.That(enumeratedIndices, Does.Contain(indices[2]));
        Assert.That(enumeratedIndices, Does.Contain(indices[4]));
        Assert.That(enumeratedIndices, Has.No.Member(indices[1]));
        Assert.That(enumeratedIndices, Has.No.Member(indices[3]));
    }

    [Test]
    public void IndexReuse_ReusesFreedIndices()
    {
        using var board = new IndexedGridBoard<ItemData>(10, 10);
        var shape = Shapes.ImmutableSingle();

        // Add 5 items
        var index0 = board.TryAddItemAt(new ItemData("Item0", 0), shape, 0, 0);
        var index1 = board.TryAddItemAt(new ItemData("Item1", 1), shape, 1, 0);
        var index2 = board.TryAddItemAt(new ItemData("Item2", 2), shape, 2, 0);
        var index3 = board.TryAddItemAt(new ItemData("Item3", 3), shape, 3, 0);
        var index4 = board.TryAddItemAt(new ItemData("Item4", 4), shape, 4, 0);

        Assert.That(index0, Is.EqualTo(0));
        Assert.That(index1, Is.EqualTo(1));
        Assert.That(index2, Is.EqualTo(2));
        Assert.That(index3, Is.EqualTo(3));
        Assert.That(index4, Is.EqualTo(4));

        // Remove indices 1, 3, 2 (in that order)
        board.RemoveItem(index1);
        board.RemoveItem(index3);
        board.RemoveItem(index2);

        var reusedIndices = new HashSet<int>();

        // Add new items - should reuse freed indices (order unspecified)
        reusedIndices.Add(board.TryAddItemAt(new ItemData("New1", 10), shape, 5, 0));
        reusedIndices.Add(board.TryAddItemAt(new ItemData("New2", 20), shape, 6, 0));
        reusedIndices.Add(board.TryAddItemAt(new ItemData("New3", 30), shape, 7, 0));

        Assert.That(reusedIndices, Is.EquivalentTo(new[] { 1, 2, 3 }), "Should reuse all freed indices");

        // Next item should get a new index
        var newIndex = board.TryAddItemAt(new ItemData("New4", 40), shape, 8, 0);
        Assert.That(newIndex, Is.EqualTo(5), "Should allocate new index after all free indices used");
    }

    [Test]
    public void GetItemById_ReturnsCorrectItemData()
    {
        using var board = new IndexedGridBoard<ItemData>(10, 10);
        var item = new ItemData("Sword", 100);
        var shape = Shapes.ImmutableSingle();

        var index = board.TryAddItemAt(item, shape, 3, 4);

        var (id, data, returnedShape, x, y) = board.GetItemById(index);

        Assert.That(id, Is.EqualTo(index));
        Assert.That(data, Is.EqualTo(item));
        Assert.That(returnedShape, Is.EqualTo(shape));
        Assert.That(x, Is.EqualTo(3));
        Assert.That(y, Is.EqualTo(4));
    }

    [Test]
    public void GetItemById_WithMultiCellShape_ReturnsCorrectData()
    {
        using var board = new IndexedGridBoard<ItemData>(10, 10);
        var item = new ItemData("LongSword", 150);
        var shape = Shapes.ImmutableLine(3);

        var index = board.TryAddItemAt(item, shape, 2, 5);

        var (id, data, returnedShape, x, y) = board.GetItemById(index);

        Assert.That(id, Is.EqualTo(index));
        Assert.That(data, Is.EqualTo(item));
        Assert.That(returnedShape, Is.EqualTo(shape));
        Assert.That(x, Is.EqualTo(2));
        Assert.That(y, Is.EqualTo(5));
    }

    [Test]
    public void GetItemOnPosition_ReturnsCorrectItemData()
    {
        using var board = new IndexedGridBoard<ItemData>(10, 10);
        var item = new ItemData("Shield", 80);
        var shape = Shapes.ImmutableSingle();

        var index = board.TryAddItemAt(item, shape, 6, 7);

        var (id, data, returnedShape, x, y) = board.GetItemOnPosition(6, 7);

        Assert.That(id, Is.EqualTo(index));
        Assert.That(data, Is.EqualTo(item));
        Assert.That(returnedShape, Is.EqualTo(shape));
        Assert.That(x, Is.EqualTo(6));
        Assert.That(y, Is.EqualTo(7));
    }

    [Test]
    public void GetItemOnPosition_WithMultiCellShape_ReturnsCorrectDataFromAnyCell()
    {
        using var board = new IndexedGridBoard<ItemData>(10, 10);
        var item = new ItemData("Spear", 120);
        var shape = Shapes.ImmutableLine(4); // 4-cell horizontal line

        var index = board.TryAddItemAt(item, shape, 1, 3);

        // Test getting item from different cells of the same shape
        var result1 = board.GetItemOnPosition(1, 3);
        var result2 = board.GetItemOnPosition(2, 3);
        var result3 = board.GetItemOnPosition(3, 3);
        var result4 = board.GetItemOnPosition(4, 3);

        // All should return the same item data
        Assert.That(result1.Data, Is.EqualTo(item));
        Assert.That(result2.Data, Is.EqualTo(item));
        Assert.That(result3.Data, Is.EqualTo(item));
        Assert.That(result4.Data, Is.EqualTo(item));

        // All should return the same ID
        Assert.That(result1.Id, Is.EqualTo(index));
        Assert.That(result2.Id, Is.EqualTo(index));
        Assert.That(result3.Id, Is.EqualTo(index));
        Assert.That(result4.Id, Is.EqualTo(index));

        // All should return the same position (origin of the shape)
        Assert.That(result1.X, Is.EqualTo(1));
        Assert.That(result2.X, Is.EqualTo(1));
        Assert.That(result3.X, Is.EqualTo(1));
        Assert.That(result4.X, Is.EqualTo(1));
    }

    [Test]
    public void GetItemById_AfterRemoval_ReturnsUpdatedData()
    {
        using var board = new IndexedGridBoard<ItemData>(10, 10);
        var item1 = new ItemData("OldItem", 50);
        var item2 = new ItemData("NewItem", 75);
        var shape = Shapes.ImmutableSingle();

        var index1 = board.TryAddItemAt(item1, shape, 2, 2);
        board.RemoveItem(index1);

        // Reuse the same index
        var index2 = board.TryAddItemAt(item2, shape, 5, 5);

        var (_, data, _, x, y) = board.GetItemById(index2);

        Assert.That(data, Is.EqualTo(item2));
        Assert.That(x, Is.EqualTo(5));
        Assert.That(y, Is.EqualTo(5));
    }

    [Test]
    public void ReadOnly_GetItemById_ReturnsCorrectData()
    {
        using var board = new IndexedGridBoard<ItemData>(10, 10);
        var item = new ItemData("Potion", 25);
        var shape = Shapes.ImmutableSingle();

        var index = board.TryAddItemAt(item, shape, 4, 4);
        var readOnly = board.AsReadOnly();

        var (id, data, returnedShape, x, y) = readOnly.GetItemById(index);

        Assert.That(id, Is.EqualTo(index));
        Assert.That(data, Is.EqualTo(item));
        Assert.That(returnedShape, Is.EqualTo(shape));
        Assert.That(x, Is.EqualTo(4));
        Assert.That(y, Is.EqualTo(4));
    }

    [Test]
    public void ReadOnly_GetItemOnPosition_ReturnsCorrectData()
    {
        using var board = new IndexedGridBoard<ItemData>(10, 10);
        var item = new ItemData("Gem", 200);
        var shape = Shapes.ImmutableSingle();

        board.TryAddItemAt(item, shape, 8, 9);
        var readOnly = board.AsReadOnly();

        var (_, data, returnedShape, x, y) = readOnly.GetItemOnPosition(8, 9);

        Assert.That(data, Is.EqualTo(item));
        Assert.That(returnedShape, Is.EqualTo(shape));
        Assert.That(x, Is.EqualTo(8));
        Assert.That(y, Is.EqualTo(9));
    }
}
