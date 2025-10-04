using DopeGrid;
using DopeGrid.Standard;
using NUnit.Framework;

public class StandardGridBoardExtensionTests
{
    [Test]
    public void FindFirstFit_FindsCorrectPosition()
    {
        var container = new GridShape(5, 5);
        var item = Shapes.Square(2);
        var immutableItem = item.GetOrCreateImmutable();

        var pos = ReadOnlyGridShapeExtension.FindFirstFitWithFixedRotation(ref container, immutableItem, freeValue: false);

        Assert.AreEqual(GridPosition.Zero, pos);
        item.Dispose();
        container.Dispose();
    }

    [Test]
    public void FindFirstFit_SkipsOccupiedAreas()
    {
        var container = new GridShape(5, 5);
        container.FillRect(0, 0, 2, 2, true);

        var item = Shapes.Square(2);
        var immutableItem = item.GetOrCreateImmutable();
        var pos = ReadOnlyGridShapeExtension.FindFirstFitWithFixedRotation(ref container, immutableItem, freeValue: false);

        Assert.AreNotEqual(new GridPosition(0, 0), pos);
        Assert.IsTrue(pos.X >= 0 && pos.Y >= 0);
        item.Dispose();
        container.Dispose();
    }

    [Test]
    public void FindFirstFit_ReturnsNegativeWhenNoFit()
    {
        var container = new GridShape(2, 2);
        container.FillAll(true);

        var item = Shapes.Single();
        var immutableItem = item.GetOrCreateImmutable();
        var pos = ReadOnlyGridShapeExtension.FindFirstFitWithFixedRotation(ref container, immutableItem, freeValue: false);

        Assert.AreEqual(new GridPosition(-1, -1), pos);
        item.Dispose();
        container.Dispose();
    }

    [Test]
    public void FindFirstFit_ItemTooLarge_ReturnsNegative()
    {
        var container = new GridShape(3, 3);
        var item = Shapes.Square(5);
        var immutableItem = item.GetOrCreateImmutable();

        var pos = ReadOnlyGridShapeExtension.FindFirstFitWithFixedRotation(ref container, immutableItem, freeValue: false);

        Assert.AreEqual(new GridPosition(-1, -1), pos);
        item.Dispose();
        container.Dispose();
    }

    [Test]
    public void CanPlaceItem_ReturnsTrueForValidPosition()
    {
        var container = new GridShape(5, 5);
        var item = Shapes.Square(2);
        var immutableItem = item.GetOrCreateImmutable();

        Assert.IsTrue(ReadOnlyGridShapeExtension.CanPlaceItem(ref container, immutableItem, new GridPosition(1, 1), freeValue: false));
        Assert.IsTrue(ReadOnlyGridShapeExtension.CanPlaceItem(ref container, immutableItem, new GridPosition(0, 0), freeValue: false));
        Assert.IsTrue(ReadOnlyGridShapeExtension.CanPlaceItem(ref container, immutableItem, new GridPosition(3, 3), freeValue: false));
        item.Dispose();
        container.Dispose();
    }

    [Test]
    public void CanPlaceItem_ReturnsFalseForOutOfBounds()
    {
        var container = new GridShape(5, 5);
        var item = Shapes.Square(2);
        var immutableItem = item.GetOrCreateImmutable();

        Assert.IsFalse(ReadOnlyGridShapeExtension.CanPlaceItem(ref container, immutableItem, new GridPosition(4, 4), freeValue: false));
        Assert.IsFalse(ReadOnlyGridShapeExtension.CanPlaceItem(ref container, immutableItem, new GridPosition(5, 0), freeValue: false));
        Assert.IsFalse(ReadOnlyGridShapeExtension.CanPlaceItem(ref container, immutableItem, new GridPosition(-1, 0), freeValue: false));
        item.Dispose();
        container.Dispose();
    }

    [Test]
    public void CanPlaceItem_ReturnsFalseForOccupiedSpace()
    {
        var container = new GridShape(5, 5);
        container[2, 2] = true;

        var item = Shapes.Square(2);
        var immutableItem = item.GetOrCreateImmutable();

        Assert.IsFalse(ReadOnlyGridShapeExtension.CanPlaceItem(ref container, immutableItem, new GridPosition(1, 1), freeValue: false));
        Assert.IsTrue(ReadOnlyGridShapeExtension.CanPlaceItem(ref container, immutableItem, new GridPosition(3, 3), freeValue: false));
        item.Dispose();
        container.Dispose();
    }

    [Test]
    public void PlaceItem_PlacesItemCorrectly()
    {
        var container = new GridShape(5, 5);
        var item = Shapes.LShape();
        var immutableItem = item.GetOrCreateImmutable();

        WritableGridShapeExtension.PlaceItem(ref container, immutableItem, new GridPosition(1, 1), true);

        Assert.IsTrue(container.GetCellValue(1, 1));
        Assert.IsTrue(container.GetCellValue(1, 2));
        Assert.IsTrue(container.GetCellValue(2, 2));
        Assert.IsFalse(container.GetCellValue(2, 1));
        Assert.IsFalse(container.GetCellValue(0, 0));
        item.Dispose();
        container.Dispose();
    }

    [Test]
    public void PlaceItem_OnlyPlacesShapeCells()
    {
        var container = new GridShape(5, 5);
        var item = Shapes.LShape();
        var immutableItem = item.GetOrCreateImmutable();

        WritableGridShapeExtension.PlaceItem(ref container, immutableItem, new GridPosition(2, 2), true);

        // L-shape at (2,2): (2,2), (2,3), (3,3)
        Assert.IsTrue(container.GetCellValue(2, 2));
        Assert.IsTrue(container.GetCellValue(2, 3));
        Assert.IsTrue(container.GetCellValue(3, 3));
        Assert.IsFalse(container.GetCellValue(3, 2));
        item.Dispose();
        container.Dispose();
    }

    [Test]
    public void RemoveItem_RemovesItemCorrectly()
    {
        var container = new GridShape(5, 5);
        var item = Shapes.Square(2);
        var immutableItem = item.GetOrCreateImmutable();

        WritableGridShapeExtension.PlaceItem(ref container, immutableItem, new GridPosition(1, 1), true);
        WritableGridShapeExtension.RemoveItem(ref container, immutableItem, new GridPosition(1, 1), freeValue: false);

        Assert.IsFalse(container.GetCellValue(1, 1));
        Assert.IsFalse(container.GetCellValue(2, 1));
        Assert.IsFalse(container.GetCellValue(1, 2));
        Assert.IsFalse(container.GetCellValue(2, 2));
        item.Dispose();
        container.Dispose();
    }

    [Test]
    public void RemoveItem_OnlyRemovesShapeCells()
    {
        var container = new GridShape(5, 5);
        container.FillAll(true);

        var item = Shapes.LShape();
        var immutableItem = item.GetOrCreateImmutable();

        WritableGridShapeExtension.RemoveItem(ref container, immutableItem, new GridPosition(2, 2), freeValue: false);

        // L-shape cells should be removed
        Assert.IsFalse(container.GetCellValue(2, 2));
        Assert.IsFalse(container.GetCellValue(2, 3));
        Assert.IsFalse(container.GetCellValue(3, 3));
        // Other cells should remain
        Assert.IsTrue(container.GetCellValue(3, 2));
        Assert.IsTrue(container.GetCellValue(0, 0));
        item.Dispose();
        container.Dispose();
    }

    [Test]
    public void PlaceMultipleShapes_PlacesAllThatFit()
    {
        var container = new GridShape(10, 10);

        var items = new[]
        {
            Shapes.Square(2).GetOrCreateImmutable(),
            Shapes.Single().GetOrCreateImmutable(),
            Shapes.Line(3).GetOrCreateImmutable()
        };

        var positions = new GridPosition[items.Length];

        // Place items manually
        var placed = 0;
        for (var i = 0; i < items.Length; i++)
        {
            var position = ReadOnlyGridShapeExtension.FindFirstFitWithFixedRotation(ref container, items[i], freeValue: false);
            if (position.IsValid)
            {
                WritableGridShapeExtension.PlaceItem(ref container, items[i], position, true);
                positions[i] = position;
                placed++;
            }
        }

        Assert.AreEqual(3, placed);

        // Verify all items were placed
        for (int i = 0; i < placed; i++)
        {
            Assert.IsTrue(positions[i].X >= 0);
            Assert.IsTrue(positions[i].Y >= 0);
        }

        container.Dispose();
    }

    [Test]
    public void PlaceMultipleShapes_StopsWhenContainerFull()
    {
        var container = new GridShape(2, 2);

        var items = new[]
        {
            Shapes.Square(2).GetOrCreateImmutable(),
            Shapes.Single().GetOrCreateImmutable()  // Won't fit after first item
        };

        var positions = new GridPosition[items.Length];

        // Place items manually
        var placed = 0;
        for (var i = 0; i < items.Length; i++)
        {
            var position = ReadOnlyGridShapeExtension.FindFirstFitWithFixedRotation(ref container, items[i], freeValue: false);
            if (position.IsValid)
            {
                WritableGridShapeExtension.PlaceItem(ref container, items[i], position, true);
                positions[i] = position;
                placed++;
            }
        }

        Assert.AreEqual(1, placed);

        container.Dispose();
    }

    [Test]
    public void CanPlaceItem_WithComplexShape_WorksCorrectly()
    {
        var container = new GridShape(5, 5);
        var tShape = Shapes.TShape();
        var immutableTShape = tShape.GetOrCreateImmutable();

        // T-shape: XXX
        //           X
        Assert.IsTrue(ReadOnlyGridShapeExtension.CanPlaceItem(ref container, immutableTShape, new GridPosition(0, 0), freeValue: false));
        Assert.IsTrue(ReadOnlyGridShapeExtension.CanPlaceItem(ref container, immutableTShape, new GridPosition(2, 3), freeValue: false));

        container[1, 0] = true;
        Assert.IsFalse(ReadOnlyGridShapeExtension.CanPlaceItem(ref container, immutableTShape, new GridPosition(0, 0), freeValue: false));
        tShape.Dispose();
        container.Dispose();
    }

    [Test]
    public void PlaceAndRemove_RoundTrip_RestoresOriginalState()
    {
        var container = new GridShape(5, 5);
        var original = container.Clone();
        var item = Shapes.Cross();
        var immutableItem = item.GetOrCreateImmutable();

        WritableGridShapeExtension.PlaceItem(ref container, immutableItem, new GridPosition(1, 1), true);
        WritableGridShapeExtension.RemoveItem(ref container, immutableItem, new GridPosition(1, 1), freeValue: false);

        for (int y = 0; y < 5; y++)
        for (int x = 0; x < 5; x++)
        {
        Assert.AreEqual(original.GetCellValue(x, y), container.GetCellValue(x, y));
        }
        item.Dispose();
        original.Dispose();
        container.Dispose();
    }
}
