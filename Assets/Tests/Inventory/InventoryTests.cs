using System;
using DopeGrid;
using DopeGrid.Inventory;
using DopeGrid.Native;
using NUnit.Framework;
using Unity.Collections;
using Unity.Mathematics;

public class InventoryTests
{
    private ItemDefinition _singleCellItem;
    private ItemDefinition _squareItem2x2;
    private ItemDefinition _lShapeItem;
    private ItemDefinition _lineItem3x1;
    private ItemDefinition _tShapeItem;

    [SetUp]
    public void Setup()
    {
        // Create test item definitions with different shapes
        _singleCellItem = new ItemDefinition(new Guid("6D5D5967-6E91-43FD-AB41-9E2004F5C021"), Shapes.ImmutableSingle());
        _squareItem2x2 = new ItemDefinition(new Guid("00BFF2FF-324E-4146-A155-9335FC42CBC3"), Shapes.ImmutableSquare(2));
        _lShapeItem = new ItemDefinition(new Guid("A85328FB-8BAE-420D-827A-69C38DB047D0"), Shapes.ImmutableLShape());
        _lineItem3x1 = new ItemDefinition(new Guid("244B8E6C-C7D2-4D13-9ED7-3BED35A41D02"), Shapes.ImmutableLine(3));
        _tShapeItem = new ItemDefinition(new Guid("7D14D41D-E4D3-483F-84BC-9FEE4A44C66B"), Shapes.ImmutableTShape());
    }

    #region Constructor and Properties Tests

    [Test]
    public void Constructor_CreatesEmptyInventory()
    {
        var inventory = new Inventory(5, 5, Allocator.Temp);

        Assert.AreEqual(5, inventory.Width);
        Assert.AreEqual(5, inventory.Height);
        Assert.AreEqual(0, inventory.ItemCount);
        Assert.IsTrue(inventory.IsEmpty);

        // Check all cells are empty (-1)
        for (int y = 0; y < 5; y++)
        {
            for (int x = 0; x < 5; x++)
            {
                Assert.AreEqual(-1, inventory.Grid[x, y]);
            }
        }

        inventory.Dispose();
    }

    [Test]
    public void DefaultConstructor_CreatesZeroSizedInventory()
    {
        var inventory = new Inventory();

        Assert.AreEqual(0, inventory.Width);
        Assert.AreEqual(0, inventory.Height);
        Assert.AreEqual(0, inventory.ItemCount);
        Assert.IsTrue(inventory.IsEmpty);

        inventory.Dispose();
    }

    [Test]
    public void Properties_UpdateCorrectlyAfterAddingItems()
    {
        var inventory = new Inventory(5, 5, Allocator.Temp);
        var item = new InventoryItem(1, _singleCellItem, RotationDegree.None, new int2(0, 0));

        Assert.IsTrue(inventory.IsEmpty);

        inventory.TryPlaceItem(item);

        Assert.IsFalse(inventory.IsEmpty);
        Assert.AreEqual(1, inventory.ItemCount);

        inventory.Dispose();
    }

    #endregion

    #region TryPlaceItem Tests

    [Test]
    public void TryPlaceItem_PlacesSingleCellItem()
    {
        var inventory = new Inventory(5, 5, Allocator.Temp);
        var item = new InventoryItem(1, _singleCellItem, RotationDegree.None, new int2(2, 2));

        var result = inventory.TryPlaceItem(item);

        Assert.IsTrue(result);
        Assert.AreEqual(1, inventory.ItemCount);
        Assert.AreEqual(0, inventory.Grid[2, 2]);  // Item index 0
        Assert.AreEqual(-1, inventory.Grid[1, 2]); // Adjacent cells empty
        Assert.AreEqual(-1, inventory.Grid[3, 2]);

        inventory.Dispose();
    }

    [Test]
    public void TryPlaceItem_PlacesSquareItem()
    {
        var inventory = new Inventory(5, 5, Allocator.Temp);
        var item = new InventoryItem(1, _squareItem2x2, RotationDegree.None, new int2(1, 1));

        var result = inventory.TryPlaceItem(item);

        Assert.IsTrue(result);
        Assert.AreEqual(0, inventory.Grid[1, 1]);
        Assert.AreEqual(0, inventory.Grid[2, 1]);
        Assert.AreEqual(0, inventory.Grid[1, 2]);
        Assert.AreEqual(0, inventory.Grid[2, 2]);

        inventory.Dispose();
    }

    [Test]
    public void TryPlaceItem_PlacesLShapeItem()
    {
        var inventory = new Inventory(5, 5, Allocator.Temp);
        var item = new InventoryItem(1, _lShapeItem, RotationDegree.None, new int2(1, 1));

        var result = inventory.TryPlaceItem(item);

        Assert.IsTrue(result);
        // L-shape occupies (1,1), (1,2), (2,2)
        Assert.AreEqual(0, inventory.Grid[1, 1]);
        Assert.AreEqual(0, inventory.Grid[1, 2]);
        Assert.AreEqual(0, inventory.Grid[2, 2]);
        Assert.AreEqual(-1, inventory.Grid[2, 1]); // Empty corner

        inventory.Dispose();
    }

    [Test]
    public void TryPlaceItem_FailsWhenOutOfBounds()
    {
        var inventory = new Inventory(5, 5, Allocator.Temp);
        var item = new InventoryItem(1, _squareItem2x2, RotationDegree.None, new int2(4, 4));

        var result = inventory.TryPlaceItem(item);

        Assert.IsFalse(result);
        Assert.AreEqual(0, inventory.ItemCount);

        inventory.Dispose();
    }

    [Test]
    public void TryPlaceItem_FailsWhenNegativePosition()
    {
        var inventory = new Inventory(5, 5, Allocator.Temp);
        var item = new InventoryItem(1, _singleCellItem, RotationDegree.None, new int2(-1, 0));

        var result = inventory.TryPlaceItem(item);

        Assert.IsFalse(result);
        Assert.AreEqual(0, inventory.ItemCount);

        inventory.Dispose();
    }

    [Test]
    public void TryPlaceItem_FailsWhenOverlapping()
    {
        var inventory = new Inventory(5, 5, Allocator.Temp);
        var item1 = new InventoryItem(1, _squareItem2x2, RotationDegree.None, new int2(1, 1));
        var item2 = new InventoryItem(2, _squareItem2x2, RotationDegree.None, new int2(2, 2));

        inventory.TryPlaceItem(item1);
        var result = inventory.TryPlaceItem(item2); // Overlaps at (2,2)

        Assert.IsFalse(result);
        Assert.AreEqual(1, inventory.ItemCount);

        inventory.Dispose();
    }

    [Test]
    public void TryPlaceItem_MultipleNonOverlappingItems()
    {
        var inventory = new Inventory(5, 5, Allocator.Temp);
        var item1 = new InventoryItem(1, _singleCellItem, RotationDegree.None, new int2(0, 0));
        var item2 = new InventoryItem(2, _singleCellItem, RotationDegree.None, new int2(2, 0));
        var item3 = new InventoryItem(3, _singleCellItem, RotationDegree.None, new int2(4, 0));

        Assert.IsTrue(inventory.TryPlaceItem(item1));
        Assert.IsTrue(inventory.TryPlaceItem(item2));
        Assert.IsTrue(inventory.TryPlaceItem(item3));

        Assert.AreEqual(3, inventory.ItemCount);
        Assert.AreEqual(0, inventory.Grid[0, 0]);
        Assert.AreEqual(1, inventory.Grid[2, 0]);
        Assert.AreEqual(2, inventory.Grid[4, 0]);

        inventory.Dispose();
    }

    #endregion

    #region RemoveItem Tests

    [Test]
    public void RemoveItem_RemovesSingleItem()
    {
        var inventory = new Inventory(5, 5, Allocator.Temp);
        var item = new InventoryItem(1, _singleCellItem, RotationDegree.None, new int2(2, 2));

        inventory.TryPlaceItem(item);
        var result = inventory.RemoveItem(0);

        Assert.IsTrue(result);
        Assert.AreEqual(0, inventory.ItemCount);
        Assert.AreEqual(-1, inventory.Grid[2, 2]); // Cell is empty again

        inventory.Dispose();
    }

    [Test]
    public void RemoveItem_RemovesSquareItem()
    {
        var inventory = new Inventory(5, 5, Allocator.Temp);
        var item = new InventoryItem(1, _squareItem2x2, RotationDegree.None, new int2(1, 1));

        inventory.TryPlaceItem(item);
        var result = inventory.RemoveItem(0);

        Assert.IsTrue(result);
        Assert.AreEqual(-1, inventory.Grid[1, 1]);
        Assert.AreEqual(-1, inventory.Grid[2, 1]);
        Assert.AreEqual(-1, inventory.Grid[1, 2]);
        Assert.AreEqual(-1, inventory.Grid[2, 2]);

        inventory.Dispose();
    }

    [Test]
    public void RemoveItem_UpdatesIndicesWhenRemovingMiddleItem()
    {
        var inventory = new Inventory(5, 5, Allocator.Temp);
        var item1 = new InventoryItem(1, _singleCellItem, RotationDegree.None, new int2(0, 0));
        var item2 = new InventoryItem(2, _singleCellItem, RotationDegree.None, new int2(2, 0));
        var item3 = new InventoryItem(3, _singleCellItem, RotationDegree.None, new int2(4, 0));

        inventory.TryPlaceItem(item1);
        inventory.TryPlaceItem(item2);
        inventory.TryPlaceItem(item3);

        // Remove middle item (index 1)
        inventory.RemoveItem(1);

        Assert.AreEqual(2, inventory.ItemCount);
        Assert.AreEqual(0, inventory.Grid[0, 0]); // First item unchanged
        Assert.AreEqual(-1, inventory.Grid[2, 0]); // Middle item removed
        Assert.AreEqual(1, inventory.Grid[4, 0]); // Last item moved to index 1

        inventory.Dispose();
    }

    [Test]
    public void RemoveItem_FailsWithInvalidIndex()
    {
        var inventory = new Inventory(5, 5, Allocator.Temp);

        Assert.IsFalse(inventory.RemoveItem(-1));
        Assert.IsFalse(inventory.RemoveItem(0));
        Assert.IsFalse(inventory.RemoveItem(5));

        inventory.Dispose();
    }

    #endregion

    #region TryMoveItem Tests

    [Test]
    public void TryMoveItem_MovesItemToNewPosition()
    {
        var inventory = new Inventory(5, 5, Allocator.Temp);
        var item = new InventoryItem(1, _singleCellItem, RotationDegree.None, new int2(0, 0));

        inventory.TryPlaceItem(item);
        var result = inventory.TryMoveItem(0, new int2(3, 3));

        Assert.IsTrue(result);
        Assert.AreEqual(-1, inventory.Grid[0, 0]); // Old position empty
        Assert.AreEqual(0, inventory.Grid[3, 3]);  // New position occupied
        Assert.AreEqual(new int2(3, 3), inventory.Items[0].Position);

        inventory.Dispose();
    }

    [Test]
    public void TryMoveItem_MovesSquareItem()
    {
        var inventory = new Inventory(5, 5, Allocator.Temp);
        var item = new InventoryItem(1, _squareItem2x2, RotationDegree.None, new int2(0, 0));

        inventory.TryPlaceItem(item);
        var result = inventory.TryMoveItem(0, new int2(2, 2));

        Assert.IsTrue(result);

        // Old position empty
        Assert.AreEqual(-1, inventory.Grid[0, 0]);
        Assert.AreEqual(-1, inventory.Grid[1, 0]);
        Assert.AreEqual(-1, inventory.Grid[0, 1]);
        Assert.AreEqual(-1, inventory.Grid[1, 1]);

        // New position occupied
        Assert.AreEqual(0, inventory.Grid[2, 2]);
        Assert.AreEqual(0, inventory.Grid[3, 2]);
        Assert.AreEqual(0, inventory.Grid[2, 3]);
        Assert.AreEqual(0, inventory.Grid[3, 3]);

        inventory.Dispose();
    }

    [Test]
    public void TryMoveItem_FailsWhenBlockedByOtherItem()
    {
        var inventory = new Inventory(5, 5, Allocator.Temp);
        var item1 = new InventoryItem(1, _singleCellItem, RotationDegree.None, new int2(0, 0));
        var item2 = new InventoryItem(2, _singleCellItem, RotationDegree.None, new int2(2, 0));

        inventory.TryPlaceItem(item1);
        inventory.TryPlaceItem(item2);

        var result = inventory.TryMoveItem(0, new int2(2, 0)); // Try to move to item2's position

        Assert.IsFalse(result);
        Assert.AreEqual(0, inventory.Grid[0, 0]); // Item1 didn't move
        Assert.AreEqual(1, inventory.Grid[2, 0]); // Item2 still there

        inventory.Dispose();
    }

    [Test]
    public void TryMoveItem_CanMoveToOverlapOwnPosition()
    {
        var inventory = new Inventory(5, 5, Allocator.Temp);
        var item = new InventoryItem(1, _squareItem2x2, RotationDegree.None, new int2(1, 1));

        inventory.TryPlaceItem(item);
        var result = inventory.TryMoveItem(0, new int2(2, 1)); // Partial overlap with self

        Assert.IsTrue(result);
        Assert.AreEqual(0, inventory.Grid[2, 1]);
        Assert.AreEqual(0, inventory.Grid[3, 1]);
        Assert.AreEqual(0, inventory.Grid[2, 2]);
        Assert.AreEqual(0, inventory.Grid[3, 2]);
        Assert.AreEqual(-1, inventory.Grid[1, 1]); // Old cells now empty

        inventory.Dispose();
    }

    [Test]
    public void TryMoveItem_FailsWhenOutOfBounds()
    {
        var inventory = new Inventory(5, 5, Allocator.Temp);
        var item = new InventoryItem(1, _squareItem2x2, RotationDegree.None, new int2(0, 0));

        inventory.TryPlaceItem(item);
        var result = inventory.TryMoveItem(0, new int2(4, 4));

        Assert.IsFalse(result);
        Assert.AreEqual(new int2(0, 0), inventory.Items[0].Position);

        inventory.Dispose();
    }

    #endregion

    #region Query Methods Tests

    [Test]
    public void GetItemAt_ReturnsCorrectItemIndex()
    {
        var inventory = new Inventory(5, 5, Allocator.Temp);
        var item1 = new InventoryItem(1, _singleCellItem, RotationDegree.None, new int2(0, 0));
        var item2 = new InventoryItem(2, _squareItem2x2, RotationDegree.None, new int2(2, 2));

        inventory.TryPlaceItem(item1);
        inventory.TryPlaceItem(item2);

        Assert.AreEqual(0, inventory.GetItemAt(new int2(0, 0)));
        Assert.AreEqual(1, inventory.GetItemAt(new int2(2, 2)));
        Assert.AreEqual(1, inventory.GetItemAt(new int2(3, 3)));
        Assert.AreEqual(-1, inventory.GetItemAt(new int2(1, 1)));

        inventory.Dispose();
    }

    [Test]
    public void GetItemAt_ReturnsMinusOneForEmptyCell()
    {
        var inventory = new Inventory(5, 5, Allocator.Temp);

        Assert.AreEqual(-1, inventory.GetItemAt(new int2(0, 0)));
        Assert.AreEqual(-1, inventory.GetItemAt(new int2(2, 2)));

        inventory.Dispose();
    }

    [Test]
    public void GetItemAt_ReturnsMinusOneForOutOfBounds()
    {
        var inventory = new Inventory(5, 5, Allocator.Temp);

        Assert.AreEqual(-1, inventory.GetItemAt(new int2(-1, 0)));
        Assert.AreEqual(-1, inventory.GetItemAt(new int2(5, 0)));
        Assert.AreEqual(-1, inventory.GetItemAt(new int2(0, 5)));

        inventory.Dispose();
    }

    [Test]
    public void IsPositionOccupied_WorksCorrectly()
    {
        var inventory = new Inventory(5, 5, Allocator.Temp);
        var item = new InventoryItem(1, _singleCellItem, RotationDegree.None, new int2(2, 2));

        Assert.IsFalse(inventory.IsPositionOccupied(new int2(2, 2)));

        inventory.TryPlaceItem(item);

        Assert.IsTrue(inventory.IsPositionOccupied(new int2(2, 2)));
        Assert.IsFalse(inventory.IsPositionOccupied(new int2(0, 0)));

        inventory.Dispose();
    }

    [Test]
    public void ContainsItem_FindsItemByInstanceId()
    {
        var inventory = new Inventory(5, 5, Allocator.Temp);
        var item1 = new InventoryItem(100, _singleCellItem, RotationDegree.None, new int2(0, 0));
        var item2 = new InventoryItem(200, _singleCellItem, RotationDegree.None, new int2(2, 0));

        inventory.TryPlaceItem(item1);
        inventory.TryPlaceItem(item2);

        Assert.IsTrue(inventory.ContainsItem(100));
        Assert.IsTrue(inventory.ContainsItem(200));
        Assert.IsFalse(inventory.ContainsItem(300));

        inventory.Dispose();
    }

    [Test]
    public void FindItemIndex_ReturnsCorrectIndex()
    {
        var inventory = new Inventory(5, 5, Allocator.Temp);
        var item1 = new InventoryItem(100, _singleCellItem, RotationDegree.None, new int2(0, 0));
        var item2 = new InventoryItem(200, _singleCellItem, RotationDegree.None, new int2(2, 0));

        inventory.TryPlaceItem(item1);
        inventory.TryPlaceItem(item2);

        Assert.AreEqual(0, inventory.GetItemIndex(100));
        Assert.AreEqual(1, inventory.GetItemIndex(200));
        Assert.AreEqual(-1, inventory.GetItemIndex(300));

        inventory.Dispose();
    }

    [Test]
    public void TryGetItem_RetrievesItemSuccessfully()
    {
        var inventory = new Inventory(5, 5, Allocator.Temp);
        var item = new InventoryItem(100, _singleCellItem, RotationDegree.None, new int2(2, 2));

        inventory.TryPlaceItem(item);

        var retrievedItem = inventory[0];
        Assert.AreEqual(100, retrievedItem.InstanceId);
        Assert.AreEqual(new int2(2, 2), retrievedItem.Position);

        inventory.Dispose();
    }

    [Test]
    public void CanPlaceItemAt_ValidatesPlacement()
    {
        var inventory = new Inventory(5, 5, Allocator.Temp);
        var item1 = new InventoryItem(1, _squareItem2x2, RotationDegree.None, new int2(1, 1));
        var item2 = new InventoryItem(2, _squareItem2x2, RotationDegree.None, new int2(0, 0));

        inventory.TryPlaceItem(item1);

        Assert.IsTrue(inventory.CanPlaceItemAt(_singleCellItem.Shape, new int2(0, 0)));
        Assert.IsFalse(inventory.CanPlaceItemAt(_squareItem2x2.Shape, new int2(2, 2))); // Overlaps
        Assert.IsFalse(inventory.CanPlaceItemAt(_squareItem2x2.Shape, new int2(4, 4))); // Out of bounds
        Assert.IsTrue(inventory.CanPlaceItemAt(item2, new int2(3, 0))); // Valid position

        inventory.Dispose();
    }

    [Test]
    public void GetFreeSpaceCount_CountsCorrectly()
    {
        var inventory = new Inventory(5, 5, Allocator.Temp);

        Assert.AreEqual(25, inventory.GetFreeSpaceCount());

        var item1 = new InventoryItem(1, _singleCellItem, RotationDegree.None, new int2(0, 0));
        var item2 = new InventoryItem(2, _squareItem2x2, RotationDegree.None, new int2(2, 2));

        inventory.TryPlaceItem(item1);
        Assert.AreEqual(24, inventory.GetFreeSpaceCount());

        inventory.TryPlaceItem(item2);
        Assert.AreEqual(20, inventory.GetFreeSpaceCount());

        inventory.Dispose();
    }

    [Test]
    public void GetOccupiedSpaceCount_CountsCorrectly()
    {
        var inventory = new Inventory(5, 5, Allocator.Temp);

        Assert.AreEqual(0, inventory.GetOccupiedSpaceCount());

        var item1 = new InventoryItem(1, _lShapeItem, RotationDegree.None, new int2(0, 0));

        inventory.TryPlaceItem(item1);
        Assert.AreEqual(3, inventory.GetOccupiedSpaceCount()); // L-shape occupies 3 cells

        inventory.Dispose();
    }

    #endregion

    #region Auto-Placement Tests

    [Test]
    public void TryFindFirstFitPosition_FindsValidPosition()
    {
        var inventory = new Inventory(5, 5, Allocator.Temp);
        var item = new InventoryItem(1, _singleCellItem, RotationDegree.None, new int2(0, 0));

        inventory.TryPlaceItem(item);

        var result = inventory.TryFindFirstFitPosition(_squareItem2x2.Shape, out var position);

        Assert.IsTrue(result);
        Assert.AreEqual(new int2(1, 0), position); // First valid position

        inventory.Dispose();
    }

    [Test]
    public void TryFindFirstFitPosition_FailsWhenNoSpace()
    {
        var inventory = new Inventory(2, 2, Allocator.Temp);
        var item = new InventoryItem(1, _squareItem2x2, RotationDegree.None, new int2(0, 0));

        inventory.TryPlaceItem(item);

        var result = inventory.TryFindFirstFitPosition(_singleCellItem.Shape, out var position);

        Assert.IsFalse(result);
        Assert.AreEqual(new int2(-1, -1), position);

        inventory.Dispose();
    }

    [Test]
    public void TryAutoPlaceItem_PlacesItemAutomatically()
    {
        var inventory = new Inventory(5, 5, Allocator.Temp);
        var item1 = new InventoryItem(1, _squareItem2x2, RotationDegree.None, new int2(0, 0));
        var item2 = new InventoryItem(2, _squareItem2x2, RotationDegree.None, new int2(-1, -1)); // Invalid position

        inventory.TryPlaceItem(item1);

        var result = inventory.TryAutoPlaceItem(item2, out var position);

        Assert.IsTrue(result);
        Assert.AreEqual(new int2(2, 0), position);
        Assert.AreEqual(2, inventory.ItemCount);
        Assert.AreEqual(position, inventory.Items[1].Position);

        inventory.Dispose();
    }

    [Test]
    public void TryAutoPlaceItem_FailsWhenNoSpaceAvailable()
    {
        var inventory = new Inventory(3, 3, Allocator.Temp);

        // Fill inventory with items
        for (int y = 0; y < 3; y++)
        {
            for (int x = 0; x < 3; x++)
            {
                var item = new InventoryItem(y * 3 + x + 1, _singleCellItem, RotationDegree.None, new int2(x, y));
                inventory.TryPlaceItem(item);
            }
        }

        var newItem = new InventoryItem(100, _singleCellItem, RotationDegree.None, new int2(-1, -1));
        var result = inventory.TryAutoPlaceItem(newItem, out var position);

        Assert.IsFalse(result);
        Assert.AreEqual(new int2(-1, -1), position);
        Assert.AreEqual(9, inventory.ItemCount);

        inventory.Dispose();
    }

    #endregion

    #region Clear and Dispose Tests

    [Test]
    public void Clear_RemovesAllItems()
    {
        var inventory = new Inventory(5, 5, Allocator.Temp);
        var item1 = new InventoryItem(1, _singleCellItem, RotationDegree.None, new int2(0, 0));
        var item2 = new InventoryItem(2, _squareItem2x2, RotationDegree.None, new int2(2, 2));

        inventory.TryPlaceItem(item1);
        inventory.TryPlaceItem(item2);

        inventory.Clear();

        Assert.AreEqual(0, inventory.ItemCount);
        Assert.IsTrue(inventory.IsEmpty);

        // Check all cells are empty
        for (int y = 0; y < 5; y++)
        {
            for (int x = 0; x < 5; x++)
            {
                Assert.AreEqual(-1, inventory.Grid[x, y]);
            }
        }

        inventory.Dispose();
    }

    [Test]
    public void Clear_AllowsPlacingItemsAfterClear()
    {
        var inventory = new Inventory(5, 5, Allocator.Temp);
        var item1 = new InventoryItem(1, _squareItem2x2, RotationDegree.None, new int2(0, 0));

        inventory.TryPlaceItem(item1);
        inventory.Clear();

        // Should be able to place item in same position after clear
        var result = inventory.TryPlaceItem(item1);

        Assert.IsTrue(result);
        Assert.AreEqual(1, inventory.ItemCount);

        inventory.Dispose();
    }

    [Test]
    public void Dispose_ReleasesResources()
    {
        var inventory = new Inventory(5, 5, Allocator.Temp);
        var item = new InventoryItem(1, _singleCellItem, RotationDegree.None, new int2(0, 0));

        inventory.TryPlaceItem(item);
        inventory.Dispose();

        // Test passes if no exceptions are thrown during dispose
        Assert.Pass();
    }

    [Test]
    public void DisposeWithJobHandle_CompletesSuccessfully()
    {
        var inventory = new Inventory(5, 5, Allocator.TempJob);
        var item = new InventoryItem(1, _singleCellItem, RotationDegree.None, new int2(0, 0));

        inventory.TryPlaceItem(item);

        var handle = new Unity.Jobs.JobHandle();
        handle = inventory.Dispose(handle);
        handle.Complete();

        Assert.Pass();
    }

    #endregion

    #region Rotation Tests

    [Test]
    public void TryPlaceItem_PlacesRotatedLineItem()
    {
        var inventory = new Inventory(5, 5, Allocator.Temp);

        // Horizontal line (3x1)
        var horizontalLine = new InventoryItem(1, _lineItem3x1, RotationDegree.None, new int2(1, 1));
        Assert.IsTrue(inventory.TryPlaceItem(horizontalLine));
        Assert.AreEqual(0, inventory.Grid[1, 1]);
        Assert.AreEqual(0, inventory.Grid[2, 1]);
        Assert.AreEqual(0, inventory.Grid[3, 1]);

        // Vertical line (1x3) - rotated 90 degrees
        var verticalLine = new InventoryItem(2, _lineItem3x1, RotationDegree.Clockwise90, new int2(0, 2));
        Assert.IsTrue(inventory.TryPlaceItem(verticalLine));
        Assert.AreEqual(1, inventory.Grid[0, 2]);
        Assert.AreEqual(1, inventory.Grid[0, 3]);
        Assert.AreEqual(1, inventory.Grid[0, 4]);

        inventory.Dispose();
    }

    [Test]
    public void TryPlaceItem_PlacesRotatedLShape()
    {
        var inventory = new Inventory(5, 5, Allocator.Temp);

        // Original L-shape
        var lShape0 = new InventoryItem(1, _lShapeItem, RotationDegree.None, new int2(0, 0));
        Assert.IsTrue(inventory.TryPlaceItem(lShape0));

        // Rotated 90 degrees
        var lShape90 = new InventoryItem(2, _lShapeItem, RotationDegree.Clockwise90, new int2(3, 0));
        Assert.IsTrue(inventory.TryPlaceItem(lShape90));

        // Rotated 180 degrees
        var lShape180 = new InventoryItem(3, _lShapeItem, RotationDegree.Clockwise180, new int2(0, 3));
        Assert.IsTrue(inventory.TryPlaceItem(lShape180));

        // Rotated 270 degrees
        var lShape270 = new InventoryItem(4, _lShapeItem, RotationDegree.Clockwise270, new int2(3, 3));
        Assert.IsTrue(inventory.TryPlaceItem(lShape270));

        Assert.AreEqual(4, inventory.ItemCount);

        inventory.Dispose();
    }

    [Test]
    public void TryPlaceItem_RotationAffectsBounds()
    {
        var inventory = new Inventory(3, 3, Allocator.Temp);

        // 3x1 line fits horizontally
        var horizontalLine = new InventoryItem(1, _lineItem3x1, RotationDegree.None, new int2(0, 0));
        Assert.IsTrue(inventory.TryPlaceItem(horizontalLine));
        inventory.Clear();

        // 3x1 line rotated 90 degrees (becomes 1x3) fits vertically
        var verticalLine = new InventoryItem(2, _lineItem3x1, RotationDegree.Clockwise90, new int2(0, 0));
        Assert.IsTrue(inventory.TryPlaceItem(verticalLine));
        inventory.Clear();

        // 3x1 line at bottom-right doesn't fit
        var lineOutOfBounds = new InventoryItem(3, _lineItem3x1, RotationDegree.None, new int2(1, 2));
        Assert.IsFalse(inventory.TryPlaceItem(lineOutOfBounds));

        inventory.Dispose();
    }

    [Test]
    public void TryMoveItem_WorksWithRotatedShapes()
    {
        var inventory = new Inventory(5, 5, Allocator.Temp);

        // Place a rotated T-shape
        var tShape = new InventoryItem(1, _tShapeItem, RotationDegree.Clockwise90, new int2(0, 0));
        Assert.IsTrue(inventory.TryPlaceItem(tShape));

        // Move it to a new position
        Assert.IsTrue(inventory.TryMoveItem(0, new int2(3, 1)));

        // Verify the shape is at the new position with correct rotation
        Assert.AreEqual(new int2(3, 1), inventory.Items[0].Position);
        Assert.AreEqual(RotationDegree.Clockwise90, inventory.Items[0].Rotation);

        inventory.Dispose();
    }

    #endregion

    #region Edge Cases and Boundary Tests

    [Test]
    public void TryPlaceItem_AtExactBoundary()
    {
        var inventory = new Inventory(3, 3, Allocator.Temp);

        // Single cell at each corner
        var topLeft = new InventoryItem(1, _singleCellItem, RotationDegree.None, new int2(0, 0));
        var topRight = new InventoryItem(2, _singleCellItem, RotationDegree.None, new int2(2, 0));
        var bottomLeft = new InventoryItem(3, _singleCellItem, RotationDegree.None, new int2(0, 2));
        var bottomRight = new InventoryItem(4, _singleCellItem, RotationDegree.None, new int2(2, 2));

        Assert.IsTrue(inventory.TryPlaceItem(topLeft));
        Assert.IsTrue(inventory.TryPlaceItem(topRight));
        Assert.IsTrue(inventory.TryPlaceItem(bottomLeft));
        Assert.IsTrue(inventory.TryPlaceItem(bottomRight));

        Assert.AreEqual(4, inventory.ItemCount);

        inventory.Dispose();
    }

    [Test]
    public void RemoveItem_LastItemDoesNotTriggerIndexUpdate()
    {
        var inventory = new Inventory(5, 5, Allocator.Temp);
        var item1 = new InventoryItem(1, _singleCellItem, RotationDegree.None, new int2(0, 0));
        var item2 = new InventoryItem(2, _singleCellItem, RotationDegree.None, new int2(2, 0));

        inventory.TryPlaceItem(item1);
        inventory.TryPlaceItem(item2);

        // Remove last item
        inventory.RemoveItem(1);

        Assert.AreEqual(1, inventory.ItemCount);
        Assert.AreEqual(0, inventory.Grid[0, 0]); // First item index unchanged
        Assert.AreEqual(-1, inventory.Grid[2, 0]); // Last item removed

        inventory.Dispose();
    }

    [Test]
    public void ComplexScenario_MultipleOperations()
    {
        var inventory = new Inventory(6, 6, Allocator.Temp);

        // Place several items
        var item1 = new InventoryItem(1, _squareItem2x2, RotationDegree.None, new int2(0, 0));
        var item2 = new InventoryItem(2, _lShapeItem, RotationDegree.Clockwise90, new int2(3, 0));
        var item3 = new InventoryItem(3, _tShapeItem, RotationDegree.None, new int2(0, 3));
        var item4 = new InventoryItem(4, _lineItem3x1, RotationDegree.Clockwise90, new int2(5, 0));

        Assert.IsTrue(inventory.TryPlaceItem(item1));
        Assert.IsTrue(inventory.TryPlaceItem(item2));
        Assert.IsTrue(inventory.TryPlaceItem(item3));
        Assert.IsTrue(inventory.TryPlaceItem(item4));

        // Move item2
        Assert.IsTrue(inventory.TryMoveItem(1, new int2(3, 3)));

        // Remove item1
        Assert.IsTrue(inventory.RemoveItem(0));

        // item4 should now be at index 0, item2 at index 1, item3 at index 2
        Assert.AreEqual(3, inventory.ItemCount);

        // Try auto-place a new item
        var item5 = new InventoryItem(5, _squareItem2x2, RotationDegree.None, new int2(-1, -1));
        Assert.IsTrue(inventory.TryAutoPlaceItem(item5, out var autoPosition));
        Assert.AreEqual(new int2(0, 0), autoPosition); // Should place where item1 was

        // Clear and verify
        inventory.Clear();
        Assert.AreEqual(0, inventory.ItemCount);
        Assert.IsTrue(inventory.IsEmpty);

        inventory.Dispose();
    }

    [Test]
    public void ZeroSizedInventory_HandlesGracefully()
    {
        var inventory = new Inventory(0, 0, Allocator.Temp);

        Assert.AreEqual(0, inventory.Width);
        Assert.AreEqual(0, inventory.Height);
        Assert.IsTrue(inventory.IsEmpty);

        var item = new InventoryItem(1, _singleCellItem, RotationDegree.None, new int2(0, 0));
        Assert.IsFalse(inventory.TryPlaceItem(item));
        Assert.IsFalse(inventory.TryFindFirstFitPosition(_singleCellItem.Shape, out _));

        inventory.Dispose();
    }

    #endregion
}
