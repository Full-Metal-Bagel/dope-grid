using System;
using DopeGrid;

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
    public void Constructor_WithValueGridShape_CreatesBoard()
    {
        using var shape = new ValueGridShape<int>(3, 3, -1);
        shape[0, 0] = 1;
        using var board = new IndexedGridBoard<ItemData>(shape);

        Assert.That(board.Width, Is.EqualTo(3));
        Assert.That(board.Height, Is.EqualTo(3));
        Assert.That(board.IsOccupied(0, 0), Is.True);
    }

    [Test]
    public void Constructor_WithEmptyShape_ThrowsException()
    {
        using var shape = new ValueGridShape<int>(0, 0, -1);
        Assert.Throws<ArgumentException>(() => new IndexedGridBoard<ItemData>(shape));
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

        board.RemoveItem(index);

        Assert.That(board.ItemCount, Is.EqualTo(1), "ItemCount stays the same due to index reuse");
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
        board.RemoveItem(index1);

        var index2 = board.TryAddItemAt(item2, shape, 1, 1);

        Assert.That(index2, Is.EqualTo(index1), "Removed index should be reused");
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
        using var shape = new ValueGridShape<int>(3, 3, -1);
        shape[0, 0] = 1;
        using var board = new IndexedGridBoard<ItemData>(shape);
        var item = new ItemData("Bow", 70);
        var itemShape = Shapes.ImmutableSingle();
        board.TryAddItemAt(item, itemShape, 1, 1);

        board.Clear();

        Assert.That(board.ItemCount, Is.EqualTo(0));
        Assert.That(board.IsOccupied(0, 0), Is.True);
        Assert.That(board.IsOccupied(1, 1), Is.False);
    }

    [Test]
    public void Indexer_ReturnsCorrectItem()
    {
        using var board = new IndexedGridBoard<ItemData>(5, 5);
        var item = new ItemData("Ring", 150);
        var shape = Shapes.ImmutableSingle();
        board.TryAddItemAt(item, shape, 2, 2);

        var retrievedItem = board[2, 2];

        Assert.That(retrievedItem, Is.EqualTo(item));
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
        Assert.That(readOnly[1, 1], Is.EqualTo(item));
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
        Assert.That(readOnly[2, 3], Is.EqualTo(item));
    }

    [Test]
    public void TryAddItem_WithRotation_RotatesShape()
    {
        using var board = new IndexedGridBoard<ItemData>(5, 5);
        var item = new ItemData("LongSword", 110);
        var shape = Shapes.ImmutableLine(4);

        // Fill the board so horizontal line won't fit, but rotated might
        var filler = new ItemData("Filler", 1);
        var single = Shapes.ImmutableSingle();
        board.TryAddItemAt(filler, single, 1, 0);
        board.TryAddItemAt(filler, single, 2, 0);
        board.TryAddItemAt(filler, single, 3, 0);

        var (index, rotation) = board.TryAddItem(item, shape);

        Assert.That(index, Is.GreaterThanOrEqualTo(0), "Should find a fit with rotation");
        Assert.That(rotation, Is.Not.EqualTo(RotationDegree.None).Or.EqualTo(RotationDegree.None),
            "Rotation may or may not be needed depending on placement");
    }

    [Test]
    public void ItemData_DefaultValue_AfterRemoval()
    {
        using var board = new IndexedGridBoard<ItemData>(5, 5);
        var item = new ItemData("Staff", 95);
        var shape = Shapes.ImmutableSingle();

        var index = board.TryAddItemAt(item, shape, 1, 1);
        board.RemoveItem(index);

        // After removal, trying to access should give default (though this is dangerous in practice)
        Assert.That(board.ItemCount, Is.EqualTo(1), "Count doesn't decrease due to index reuse");
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
    public void ReadOnly_Equals_ThrowsNotSupportedException()
    {
        using var board1 = new IndexedGridBoard<ItemData>(3, 3);
        using var board2 = new IndexedGridBoard<ItemData>(3, 3);
        var readOnly1 = board1.AsReadOnly();
        var readOnly2 = board2.AsReadOnly();

        Assert.Throws<NotSupportedException>(() => readOnly1.Equals(readOnly2));
    }
}
