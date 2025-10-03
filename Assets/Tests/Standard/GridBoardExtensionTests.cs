using DopeGrid;
using DopeGrid.Standard;
using NUnit.Framework;

public class StandardGridBoardExtensionTests
{
    [Test]
    public void FindFirstFit_FindsCorrectPosition()
    {
        using var container = new GridShape(5, 5);
        using var item = Shapes.Square(2);

        var pos = container.FindFirstFit(item.AsReadOnly());

        Assert.AreEqual(GridPosition.Zero, pos);
    }

    [Test]
    public void FindFirstFit_SkipsOccupiedAreas()
    {
        using var container = new GridShape(5, 5);
        container.FillRect(0, 0, 2, 2, true);

        using var item = Shapes.Square(2);
        var pos = container.FindFirstFit(item.AsReadOnly());

        Assert.AreNotEqual((0, 0), pos);
        Assert.IsTrue(pos.X >= 0 && pos.Y >= 0);
    }

    [Test]
    public void FindFirstFit_ReturnsNegativeWhenNoFit()
    {
        using var container = new GridShape(2, 2);
        container.FillAll(true);

        using var item = Shapes.Single();
        var pos = container.FindFirstFit(item.AsReadOnly());

        Assert.AreEqual(new GridPosition(-1, -1), pos);
    }

    [Test]
    public void FindFirstFit_ItemTooLarge_ReturnsNegative()
    {
        using var container = new GridShape(3, 3);
        using var item = Shapes.Square(5);

        var pos = container.FindFirstFit(item.AsReadOnly());

        Assert.AreEqual(new GridPosition(-1, -1), pos);
    }

    [Test]
    public void CanPlaceItem_ReturnsTrueForValidPosition()
    {
        using var container = new GridShape(5, 5);
        using var item = Shapes.Square(2);

        Assert.IsTrue(container.CanPlaceItem(item.AsReadOnly(), (1, 1)));
        Assert.IsTrue(container.CanPlaceItem(item.AsReadOnly(), (0, 0)));
        Assert.IsTrue(container.CanPlaceItem(item.AsReadOnly(), (3, 3)));
    }

    [Test]
    public void CanPlaceItem_ReturnsFalseForOutOfBounds()
    {
        using var container = new GridShape(5, 5);
        using var item = Shapes.Square(2);

        Assert.IsFalse(container.CanPlaceItem(item.AsReadOnly(), (4, 4)));
        Assert.IsFalse(container.CanPlaceItem(item.AsReadOnly(), (5, 0)));
        Assert.IsFalse(container.CanPlaceItem(item.AsReadOnly(), (-1, 0)));
    }

    [Test]
    public void CanPlaceItem_ReturnsFalseForOccupiedSpace()
    {
        using var container = new GridShape(5, 5);
        container.SetCell((2, 2), true);

        using var item = Shapes.Square(2);

        Assert.IsFalse(container.CanPlaceItem(item.AsReadOnly(), (1, 1)));
        Assert.IsTrue(container.CanPlaceItem(item.AsReadOnly(), (3, 3)));
    }

    [Test]
    public void PlaceItem_PlacesItemCorrectly()
    {
        using var container = new GridShape(5, 5);
        using var item = Shapes.LShape();

        container.PlaceItem(item.AsReadOnly(), (1, 1));

        Assert.IsTrue(container.GetCell((1, 1)));
        Assert.IsTrue(container.GetCell((1, 2)));
        Assert.IsTrue(container.GetCell((2, 2)));
        Assert.IsFalse(container.GetCell((2, 1)));
        Assert.IsFalse(container.GetCell((0, 0)));
    }

    [Test]
    public void PlaceItem_OnlyPlacesShapeCells()
    {
        using var container = new GridShape(5, 5);
        using var item = Shapes.LShape();

        container.PlaceItem(item.AsReadOnly(), (2, 2));

        // L-shape at (2,2): (2,2), (2,3), (3,3)
        Assert.IsTrue(container.GetCell((2, 2)));
        Assert.IsTrue(container.GetCell((2, 3)));
        Assert.IsTrue(container.GetCell((3, 3)));
        Assert.IsFalse(container.GetCell((3, 2)));
    }

    [Test]
    public void RemoveItem_RemovesItemCorrectly()
    {
        using var container = new GridShape(5, 5);
        using var item = Shapes.Square(2);

        container.PlaceItem(item.AsReadOnly(), (1, 1));
        container.RemoveItem(item.AsReadOnly(), (1, 1));

        Assert.IsFalse(container.GetCell((1, 1)));
        Assert.IsFalse(container.GetCell((2, 1)));
        Assert.IsFalse(container.GetCell((1, 2)));
        Assert.IsFalse(container.GetCell((2, 2)));
    }

    [Test]
    public void RemoveItem_OnlyRemovesShapeCells()
    {
        using var container = new GridShape(5, 5);
        container.FillAll(true);

        using var item = Shapes.LShape();

        container.RemoveItem(item.AsReadOnly(), (2, 2));

        // L-shape cells should be removed
        Assert.IsFalse(container.GetCell((2, 2)));
        Assert.IsFalse(container.GetCell((2, 3)));
        Assert.IsFalse(container.GetCell((3, 3)));
        // Other cells should remain
        Assert.IsTrue(container.GetCell((3, 2)));
        Assert.IsTrue(container.GetCell((0, 0)));
    }

    [Test]
    public void PlaceMultipleShapes_PlacesAllThatFit()
    {
        using var container = new GridShape(10, 10);

        var items = new[]
        {
            Shapes.Square(2),
            Shapes.Single(),
            Shapes.Line(3)
        };

        var positions = new GridPosition[items.Length];
        var placed = container.PlaceMultipleShapes(items, positions);

        Assert.AreEqual(3, placed);

        // Verify all items were placed
        for (int i = 0; i < placed; i++)
        {
            Assert.IsTrue(positions[i].X >= 0);
            Assert.IsTrue(positions[i].Y >= 0);
        }

        foreach (var item in items)
        {
            item.Dispose();
        }
    }

    [Test]
    public void PlaceMultipleShapes_StopsWhenContainerFull()
    {
        using var container = new GridShape(2, 2);

        var items = new[]
        {
            Shapes.Square(2),
            Shapes.Single()  // Won't fit after first item
        };

        var positions = new GridPosition[items.Length];
        var placed = container.PlaceMultipleShapes(items, positions);

        Assert.AreEqual(1, placed);

        foreach (var item in items)
        {
            item.Dispose();
        }
    }

    [Test]
    public void CanPlaceItem_WithComplexShape_WorksCorrectly()
    {
        using var container = new GridShape(5, 5);
        using var tShape = Shapes.TShape();

        // T-shape: XXX
        //           X
        Assert.IsTrue(container.CanPlaceItem(tShape.AsReadOnly(), (0, 0)));
        Assert.IsTrue(container.CanPlaceItem(tShape.AsReadOnly(), (2, 3)));

        container.SetCell((1, 0), true);
        Assert.IsFalse(container.CanPlaceItem(tShape.AsReadOnly(), (0, 0)));
    }

    [Test]
    public void PlaceAndRemove_RoundTrip_RestoresOriginalState()
    {
        using var container = new GridShape(5, 5);
        using var original = container.Clone();
        using var item = Shapes.Cross();

        container.PlaceItem(item.AsReadOnly(), (1, 1));
        container.RemoveItem(item.AsReadOnly(), (1, 1));

        for (int y = 0; y < 5; y++)
        for (int x = 0; x < 5; x++)
        {
            Assert.AreEqual(original.GetCell((x, y)), container.GetCell((x, y)));
        }
    }
}
