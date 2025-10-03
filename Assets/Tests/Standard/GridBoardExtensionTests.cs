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
        try
        {
            var pos = container.FindFirstFitWithFixedRotation(item.GetOrCreateImmutable(), freeValue: false);

            Assert.AreEqual(GridPosition.Zero, pos);
        }
        finally
        {
            container.Dispose();
            item.Dispose();
        }
    }

    [Test]
    public void FindFirstFit_SkipsOccupiedAreas()
    {
        var container = new GridShape(5, 5);
        var item = Shapes.Square(2);
        try
        {
            container.FillRect(0, 0, 2, 2, true);

            var pos = container.FindFirstFitWithFixedRotation(item.GetOrCreateImmutable(), freeValue: false);

            Assert.AreNotEqual((0, 0), pos);
            Assert.IsTrue(pos.X >= 0 && pos.Y >= 0);
        }
        finally
        {
            container.Dispose();
            item.Dispose();
        }
    }

    [Test]
    public void FindFirstFit_ReturnsNegativeWhenNoFit()
    {
        var container = new GridShape(2, 2);
        var item = Shapes.Single();
        try
        {
            container.FillAll(true);

            var pos = container.FindFirstFitWithFixedRotation(item.GetOrCreateImmutable(), freeValue: false);

            Assert.AreEqual(new GridPosition(-1, -1), pos);
        }
        finally
        {
            container.Dispose();
            item.Dispose();
        }
    }

    [Test]
    public void FindFirstFit_ItemTooLarge_ReturnsNegative()
    {
        var container = new GridShape(3, 3);
        var item = Shapes.Square(5);
        try
        {
            var pos = container.FindFirstFitWithFixedRotation(item.GetOrCreateImmutable(), freeValue: false);

            Assert.AreEqual(new GridPosition(-1, -1), pos);
        }
        finally
        {
            container.Dispose();
            item.Dispose();
        }
    }

    [Test]
    public void CanPlaceItem_ReturnsTrueForValidPosition()
    {
        var container = new GridShape(5, 5);
        var item = Shapes.Square(2);
        try
        {
            Assert.IsTrue(container.CanPlaceItem(item.GetOrCreateImmutable(), new GridPosition(1, 1), freeValue: false));
            Assert.IsTrue(container.CanPlaceItem(item.GetOrCreateImmutable(), new GridPosition(0, 0), freeValue: false));
            Assert.IsTrue(container.CanPlaceItem(item.GetOrCreateImmutable(), new GridPosition(3, 3), freeValue: false));
        }
        finally
        {
            container.Dispose();
            item.Dispose();
        }
    }

    [Test]
    public void CanPlaceItem_ReturnsFalseForOutOfBounds()
    {
        var container = new GridShape(5, 5);
        var item = Shapes.Square(2);
        try
        {
            Assert.IsFalse(container.CanPlaceItem(item.GetOrCreateImmutable(), new GridPosition(4, 4), freeValue: false));
            Assert.IsFalse(container.CanPlaceItem(item.GetOrCreateImmutable(), new GridPosition(5, 0), freeValue: false));
            Assert.IsFalse(container.CanPlaceItem(item.GetOrCreateImmutable(), new GridPosition(-1, 0), freeValue: false));
        }
        finally
        {
            container.Dispose();
            item.Dispose();
        }
    }

    [Test]
    public void CanPlaceItem_ReturnsFalseForOccupiedSpace()
    {
        var container = new GridShape(5, 5);
        var item = Shapes.Square(2);
        try
        {
            container[2, 2] = true;

            Assert.IsFalse(container.CanPlaceItem(item.GetOrCreateImmutable(), new GridPosition(1, 1), freeValue: false));
            Assert.IsTrue(container.CanPlaceItem(item.GetOrCreateImmutable(), new GridPosition(3, 3), freeValue: false));
        }
        finally
        {
            container.Dispose();
            item.Dispose();
        }
    }

    [Test]
    public void PlaceItem_PlacesItemCorrectly()
    {
        var container = new GridShape(5, 5);
        var item = Shapes.LShape();
        try
        {
            container.PlaceItem(item.GetOrCreateImmutable(), new GridPosition(1, 1), value: true);

            Assert.IsTrue(container[1, 1]);
            Assert.IsTrue(container[1, 2]);
            Assert.IsTrue(container[2, 2]);
            Assert.IsFalse(container[2, 1]);
            Assert.IsFalse(container[0, 0]);
        }
        finally
        {
            container.Dispose();
            item.Dispose();
        }
    }

    [Test]
    public void PlaceItem_OnlyPlacesShapeCells()
    {
        var container = new GridShape(5, 5);
        var item = Shapes.LShape();
        try
        {
            container.PlaceItem(item.GetOrCreateImmutable(), new GridPosition(2, 2), value: true);

            // L-shape at (2,2): (2,2), (2,3), (3,3)
            Assert.IsTrue(container[2, 2]);
            Assert.IsTrue(container[2, 3]);
            Assert.IsTrue(container[3, 3]);
            Assert.IsFalse(container[3, 2]);
        }
        finally
        {
            container.Dispose();
            item.Dispose();
        }
    }

    [Test]
    public void RemoveItem_RemovesItemCorrectly()
    {
        var container = new GridShape(5, 5);
        var item = Shapes.Square(2);
        try
        {
            container.PlaceItem(item.GetOrCreateImmutable(), new GridPosition(1, 1), value: true);
            container.RemoveItem(item.GetOrCreateImmutable(), new GridPosition(1, 1), freeValue: false);

            Assert.IsFalse(container[1, 1]);
            Assert.IsFalse(container[2, 1]);
            Assert.IsFalse(container[1, 2]);
            Assert.IsFalse(container[2, 2]);
        }
        finally
        {
            container.Dispose();
            item.Dispose();
        }
    }

    [Test]
    public void RemoveItem_OnlyRemovesShapeCells()
    {
        var container = new GridShape(5, 5);
        var item = Shapes.LShape();
        try
        {
            container.FillAll(true);

            container.RemoveItem(item.GetOrCreateImmutable(), new GridPosition(2, 2), freeValue: false);

            // L-shape cells should be removed
            Assert.IsFalse(container[2, 2]);
            Assert.IsFalse(container[2, 3]);
            Assert.IsFalse(container[3, 3]);
            // Other cells should remain
            Assert.IsTrue(container[3, 2]);
            Assert.IsTrue(container[0, 0]);
        }
        finally
        {
            container.Dispose();
            item.Dispose();
        }
    }

    [Test]
    public void CanPlaceItem_WithComplexShape_WorksCorrectly()
    {
        var container = new GridShape(5, 5);
        var tShape = Shapes.TShape();
        try
        {
            // T-shape: XXX
            //           X
            Assert.IsTrue(container.CanPlaceItem(tShape.GetOrCreateImmutable(), new GridPosition(0, 0), freeValue: false));
            Assert.IsTrue(container.CanPlaceItem(tShape.GetOrCreateImmutable(), new GridPosition(2, 3), freeValue: false));

            container[1, 0] = true;
            Assert.IsFalse(container.CanPlaceItem(tShape.GetOrCreateImmutable(), new GridPosition(0, 0), freeValue: false));
        }
        finally
        {
            container.Dispose();
            tShape.Dispose();
        }
    }

    [Test]
    public void PlaceAndRemove_RoundTrip_RestoresOriginalState()
    {
        var container = new GridShape(5, 5);
        var original = container.Clone();
        var item = Shapes.Cross();
        try
        {
            container.PlaceItem(item.GetOrCreateImmutable(), new GridPosition(1, 1), value: true);
            container.RemoveItem(item.GetOrCreateImmutable(), new GridPosition(1, 1), freeValue: false);

            for (int y = 0; y < 5; y++)
            for (int x = 0; x < 5; x++)
            {
                Assert.AreEqual(original[x, y], container[x, y]);
            }
        }
        finally
        {
            container.Dispose();
            original.Dispose();
            item.Dispose();
        }
    }
}
