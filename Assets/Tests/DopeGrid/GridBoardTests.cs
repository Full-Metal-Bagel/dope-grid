using System;
using NUnit.Framework;

namespace DopeGrid.Tests;

#pragma warning disable CS0618 // Type or member is obsolete

[TestFixture]
public class GridBoardTests
{
    [Test]
    public void Constructor_WithDimensions_CreatesBoard()
    {
        using var board = new GridBoard(5, 5);

        Assert.That(board.Width, Is.EqualTo(5));
        Assert.That(board.Height, Is.EqualTo(5));
        Assert.That(board.ItemCount, Is.EqualTo(0));
    }

    [Test]
    public void Constructor_WithGridShape_CreatesBoard()
    {
        using var shape = new GridShape(3, 3);
        shape[0, 0] = true;
        using var board = new GridBoard(shape);

        Assert.That(board.Width, Is.EqualTo(3));
        Assert.That(board.Height, Is.EqualTo(3));
        Assert.That(board[0, 0], Is.True);
    }

    [Test]
    public void Constructor_WithEmptyShape_ThrowsException()
    {
        using var shape = new GridShape(0, 0);
        Assert.Throws<System.ArgumentException>(() => new GridBoard(shape));
    }

    [Test]
    public void Constructor_CopyConstructor_CreatesIndependentCopy()
    {
        using var original = new GridBoard(3, 3);
        var item = Shapes.ImmutableSingle();
        original.TryAddItemAt(item, 0, 0);

        using var copy = new GridBoard(original);

        Assert.That(copy.Width, Is.EqualTo(original.Width));
        Assert.That(copy.Height, Is.EqualTo(original.Height));
        Assert.That(copy.ItemCount, Is.EqualTo(original.ItemCount));
    }

    [Test]
    public void TryAddItem_WithValidItem_ReturnsTrue()
    {
        using var board = new GridBoard(5, 5);
        var item = Shapes.ImmutableSingle();

        var result = board.TryAddItem(item);

        Assert.That(result, Is.True);
        Assert.That(board.ItemCount, Is.EqualTo(1));
    }

    [Test]
    public void TryAddItem_WithNoSpace_ReturnsFalse()
    {
        using var board = new GridBoard(1, 1);
        var item1 = Shapes.ImmutableSingle();
        var item2 = Shapes.ImmutableSingle();

        var added1 = board.TryAddItem(item1);
        var added2 = board.TryAddItem(item2);

        Assert.That(added1, Is.True, "First item should be added");
        Assert.That(added2, Is.False, "Second item should fail");
        Assert.That(board.ItemCount, Is.EqualTo(1));
    }

    [Test]
    public void TryAddItemAt_ValidPosition_ReturnsTrue()
    {
        using var board = new GridBoard(5, 5);
        var item = Shapes.ImmutableSingle();

        var result = board.TryAddItemAt(item, 2, 3);

        Assert.That(result, Is.True);
        Assert.That(board.ItemCount, Is.EqualTo(1));
        Assert.That(board.ItemPositions[0], Is.EqualTo((2, 3)));
    }

    [Test]
    public void TryAddItemAt_InvalidPosition_ReturnsFalse()
    {
        using var board = new GridBoard(5, 5);
        var item = Shapes.ImmutableLine(3);

        var result = board.TryAddItemAt(item, 4, 0);

        Assert.That(result, Is.False);
        Assert.That(board.ItemCount, Is.EqualTo(0));
    }

    [Test]
    public void TryAddItemAt_OverlappingPosition_ReturnsFalse()
    {
        using var board = new GridBoard(5, 5);
        var item1 = Shapes.ImmutableSingle();
        var item2 = Shapes.ImmutableSingle();

        board.TryAddItemAt(item1, 2, 2);
        var result = board.TryAddItemAt(item2, 2, 2);

        Assert.That(result, Is.False);
        Assert.That(board.ItemCount, Is.EqualTo(1));
    }

    [Test]
    public void RemoveItem_ValidIndex_RemovesItem()
    {
        using var board = new GridBoard(5, 5);
        var item = Shapes.ImmutableSingle();
        board.TryAddItemAt(item, 0, 0);

        Assert.That(board[0, 0], Is.True, "Cell should be occupied before removal");

        board.RemoveItem(0);

        Assert.That(board.ItemCount, Is.EqualTo(0));
        Assert.That(board[0, 0], Is.False, "Cell should be free after removal");
    }

    [Test]
    public void RemoveItem_InvalidIndex_DoesNothing()
    {
        using var board = new GridBoard(5, 5);
        var item = Shapes.ImmutableSingle();
        var added = board.TryAddItemAt(item, 0, 0);

        Assert.That(added, Is.True, "Item should be added successfully");

        board.RemoveItem(5);

        Assert.That(board.ItemCount, Is.EqualTo(1));
    }

    [Test]
    public void RemoveItem_UsesSwapBackRemoval()
    {
        using var board = new GridBoard(5, 5);
        var item1 = Shapes.ImmutableSingle();
        var item2 = Shapes.ImmutableSingle();
        var item3 = Shapes.ImmutableSingle();

        var added1 = board.TryAddItemAt(item1, 0, 0);
        var added2 = board.TryAddItemAt(item2, 1, 0);
        var added3 = board.TryAddItemAt(item3, 2, 0);

        Assert.That(added1 && added2 && added3, Is.True, "All items should be added");

        board.RemoveItem(1);

        Assert.That(board.ItemCount, Is.EqualTo(2));
        Assert.That(board.Items[1], Is.EqualTo(item3));
    }

    [Test]
    public void Clear_RemovesAllItems()
    {
        using var board = new GridBoard(5, 5);
        var item = Shapes.ImmutableSingle();
        board.TryAddItem(item);
        board.TryAddItem(item);

        board.Clear();

        Assert.That(board.ItemCount, Is.EqualTo(0));
    }

    [Test]
    public void Clear_RestoresInitializedGrid()
    {
        using var shape = new GridShape(3, 3);
        shape[0, 0] = true;
        using var board = new GridBoard(shape);
        var item = Shapes.ImmutableSingle();
        board.TryAddItemAt(item, 1, 1);

        board.Clear();

        Assert.That(board.ItemCount, Is.EqualTo(0));
        Assert.That(board[0, 0], Is.True);
        Assert.That(board[1, 1], Is.False);
    }

    [Test]
    public void Items_ReturnsCorrectList()
    {
        using var board = new GridBoard(5, 5);
        var item1 = Shapes.ImmutableSingle();
        var item2 = Shapes.ImmutableLine(2);

        board.TryAddItem(item1);
        board.TryAddItem(item2);

        Assert.That(board.Items.Count, Is.EqualTo(2));
        Assert.That(board.Items[0], Is.EqualTo(item1));
        Assert.That(board.Items[1], Is.EqualTo(item2));
    }

    [Test]
    public void ItemPositions_ReturnsCorrectList()
    {
        using var board = new GridBoard(5, 5);
        var item = Shapes.ImmutableSingle();

        var added1 = board.TryAddItemAt(item, 0, 0);
        var added2 = board.TryAddItemAt(item, 2, 3);

        Assert.That(added1 && added2, Is.True, "Both items should be added");
        Assert.That(board.ItemPositions.Count, Is.EqualTo(2));
        Assert.That(board.ItemPositions[0], Is.EqualTo((0, 0)));
        Assert.That(board.ItemPositions[1], Is.EqualTo((2, 3)));
    }

    [Test]
    public void CurrentGrid_ReflectsAddedItems()
    {
        using var board = new GridBoard(5, 5);
        var item = Shapes.ImmutableLine(3);
        board.TryAddItemAt(item, 1, 1);

        var grid = board.CurrentGrid;

        Assert.That(grid[1, 1], Is.True);
        Assert.That(grid[2, 1], Is.True);
        Assert.That(grid[3, 1], Is.True);
        Assert.That(grid[0, 0], Is.False);
    }

    [Test]
    public void IsOccupied_ReturnsCorrectValue()
    {
        using var board = new GridBoard(5, 5);
        var item = Shapes.ImmutableSingle();
        board.TryAddItemAt(item, 2, 2);

        Assert.That(board.IsOccupied(2, 2), Is.True);
        Assert.That(board.IsOccupied(0, 0), Is.False);
    }

    [Test]
    public void Clone_CreatesIndependentCopy()
    {
        using var original = new GridBoard(5, 5);
        var item = Shapes.ImmutableSingle();
        original.TryAddItemAt(item, 0, 0);

        using var clone = original.Clone();
        clone.TryAddItemAt(item, 1, 1);

        Assert.That(original.ItemCount, Is.EqualTo(1));
        Assert.That(clone.ItemCount, Is.EqualTo(2));
    }

    [Test]
    public void MultipleItems_WithComplexShapes_Work()
    {
        using var board = new GridBoard(10, 10);
        var line = Shapes.ImmutableLine(3);
        var lShape = Shapes.ImmutableLShape();
        var tShape = Shapes.ImmutableTShape();

        var added1 = board.TryAddItemAt(line, 0, 0);
        var added2 = board.TryAddItemAt(lShape, 0, 1);
        var added3 = board.TryAddItemAt(tShape, 3, 0);

        Assert.That(added1, Is.True, $"Line should be added. Line occupied count: {line.OccupiedSpaceCount()}");
        Assert.That(added2, Is.True, $"LShape should be added. LShape occupied count: {lShape.OccupiedSpaceCount()}");
        Assert.That(added3, Is.True, $"TShape should be added. TShape occupied count: {tShape.OccupiedSpaceCount()}");

        Assert.That(board.ItemCount, Is.EqualTo(3));
    }

    [Test]
    public void Dispose_SecondCallThrowsException()
    {
        var board = new GridBoard(5, 5);
        board.Dispose();

        Assert.Throws<InvalidOperationException>(() => board.Dispose(),
            "Second dispose should throw because arrays were already returned to pool");
    }

    [Test]
    public void InitializedGrid_PreservesOriginalState()
    {
        using var shape = new GridShape(3, 3);
        shape[0, 0] = true;
        shape[2, 2] = true;

        using var board = new GridBoard(shape);
        var initializedGrid = board.InitializedGrid;

        Assert.That(initializedGrid[0, 0], Is.True);
        Assert.That(initializedGrid[2, 2], Is.True);
        Assert.That(initializedGrid[1, 1], Is.False);
    }

    [Test]
    public void GetHashCode_ThrowsNotSupportedException()
    {
        using var board = new GridBoard(3, 3);

        Assert.Throws<NotSupportedException>(() => board.GetHashCode());
    }

    [Test]
    public void Equals_Object_ThrowsNotSupportedException()
    {
        using var board = new GridBoard(3, 3);

        Assert.Throws<NotSupportedException>(() => board.Equals((object)board));
    }

    [Test]
    public void Equals_GridBoard_ThrowsNotSupportedException()
    {
        using var board1 = new GridBoard(3, 3);
        using var board2 = new GridBoard(3, 3);

        Assert.Throws<NotSupportedException>(() => board1.Equals(board2));
    }

    [Test]
    public void EqualityOperator_CallsEquals()
    {
        using var board1 = new GridBoard(3, 3);
        using var board2 = new GridBoard(3, 3);

        // This will throw because Equals throws
        Assert.Throws<NotSupportedException>(() => { var _ = board1 == board2; });
    }

    [Test]
    public void InequalityOperator_CallsEquals()
    {
        using var board1 = new GridBoard(3, 3);
        using var board2 = new GridBoard(3, 3);

        // This will throw because Equals throws
        Assert.Throws<NotSupportedException>(() => { var _ = board1 != board2; });
    }
}
