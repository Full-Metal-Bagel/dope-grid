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

        Assert.IsTrue(inventory.IsEmpty);

        inventory.TryPlaceItem(_singleCellItem, RotationDegree.None, new GridPosition(0, 0));

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

        var result = inventory.TryPlaceItem(_singleCellItem, RotationDegree.None, new GridPosition(2, 2));

        Assert.IsTrue(result.IsValid);
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

        var result = inventory.TryPlaceItem(_squareItem2x2, RotationDegree.None, new GridPosition(1, 1));

        Assert.IsTrue(result.IsValid);
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

        var result = inventory.TryPlaceItem(_lShapeItem, RotationDegree.None, new GridPosition(1, 1));

        Assert.IsTrue(result.IsValid);
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

        var result = inventory.TryPlaceItem(_squareItem2x2, RotationDegree.None, new GridPosition(4, 4));

        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(0, inventory.ItemCount);

        inventory.Dispose();
    }

    [Test]
    public void TryPlaceItem_FailsWhenNegativePosition()
    {
        var inventory = new Inventory(5, 5, Allocator.Temp);

        var result = inventory.TryPlaceItem(_singleCellItem, RotationDegree.None, new GridPosition(-1, 0));

        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(0, inventory.ItemCount);

        inventory.Dispose();
    }

    [Test]
    public void TryPlaceItem_FailsWhenOverlapping()
    {
        var inventory = new Inventory(5, 5, Allocator.Temp);

        inventory.TryPlaceItem(_squareItem2x2, RotationDegree.None, new GridPosition(1, 1));
        var result = inventory.TryPlaceItem(_squareItem2x2, RotationDegree.None, new GridPosition(2, 2)); // Overlaps at (2,2)

        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, inventory.ItemCount);

        inventory.Dispose();
    }

    [Test]
    public void TryPlaceItem_MultipleNonOverlappingItems()
    {
        var inventory = new Inventory(5, 5, Allocator.Temp);

        var item1 = inventory.TryPlaceItem(_singleCellItem, RotationDegree.None, new GridPosition(0, 0));
        var item2 = inventory.TryPlaceItem(_singleCellItem, RotationDegree.None, new GridPosition(2, 0));
        var item3 = inventory.TryPlaceItem(_singleCellItem, RotationDegree.None, new GridPosition(4, 0));

        Assert.IsTrue(item1.IsValid);
        Assert.IsTrue(item2.IsValid);
        Assert.IsTrue(item3.IsValid);

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
        var item = inventory.TryPlaceItem(_singleCellItem, RotationDegree.None, new GridPosition(2, 2));

        Assert.IsTrue(item.IsValid);
        var result = inventory.RemoveItem(item.InstanceId);

        Assert.IsTrue(result);
        Assert.AreEqual(0, inventory.ItemCount);
        Assert.AreEqual(-1, inventory.Grid[2, 2]); // Cell is empty again

        inventory.Dispose();
    }

    [Test]
    public void RemoveItem_RemovesSquareItem()
    {
        var inventory = new Inventory(5, 5, Allocator.Temp);
        var item = inventory.TryPlaceItem(_squareItem2x2, RotationDegree.None, new GridPosition(1, 1));

        Assert.IsTrue(item.IsValid);
        var result = inventory.RemoveItem(item.InstanceId);

        Assert.IsTrue(result);
        Assert.AreEqual(-1, inventory.Grid[1, 1]);
        Assert.AreEqual(-1, inventory.Grid[2, 1]);
        Assert.AreEqual(-1, inventory.Grid[1, 2]);
        Assert.AreEqual(-1, inventory.Grid[2, 2]);

        inventory.Dispose();
    }

    [Test]
    public void RemoveItem_StableIndicesWhenRemovingMiddleItem()
    {
        var inventory = new Inventory(5, 5, Allocator.Temp);
        var item1 = inventory.TryPlaceItem(_singleCellItem, RotationDegree.None, new GridPosition(0, 0));
        var item2 = inventory.TryPlaceItem(_singleCellItem, RotationDegree.None, new GridPosition(2, 0));
        var item3 = inventory.TryPlaceItem(_singleCellItem, RotationDegree.None, new GridPosition(4, 0));

        Assert.IsTrue(item1.IsValid);
        Assert.IsTrue(item2.IsValid);
        Assert.IsTrue(item3.IsValid);

        // Remove middle item
        inventory.RemoveItem(item2.InstanceId);

        Assert.AreEqual(2, inventory.ItemCount);
        Assert.AreEqual(0, inventory.Grid[0, 0]); // First item still at index 0
        Assert.AreEqual(-1, inventory.Grid[2, 0]); // Middle item removed
        Assert.AreEqual(2, inventory.Grid[4, 0]); // Third item still at index 2 (stable)

        inventory.Dispose();
    }

    [Test]
    public void RemoveItem_FailsWithInvalidIndex()
    {
        var inventory = new Inventory(5, 5, Allocator.Temp);

        Assert.IsFalse(inventory.RemoveItem(InventoryItemInstanceId.Invalid));
        Assert.IsFalse(inventory.RemoveItem((InventoryItemInstanceId)0));
        Assert.IsFalse(inventory.RemoveItem((InventoryItemInstanceId)5));

        inventory.Dispose();
    }

    #endregion

    #region TryMoveItem Tests

    [Test]
    public void TryMoveItem_MovesItemToNewPosition()
    {
        var inventory = new Inventory(5, 5, Allocator.Temp);
        var item = inventory.TryPlaceItem(_singleCellItem, RotationDegree.None, new GridPosition(0, 0));

        Assert.IsTrue(item.IsValid);
        var result = inventory.TryMoveItem(item.InstanceId, new GridPosition(3, 3));

        Assert.IsTrue(result);
        Assert.AreEqual(-1, inventory.Grid[0, 0]); // Old position empty
        Assert.AreEqual(0, inventory.Grid[3, 3]);  // New position occupied
        Assert.AreEqual(new GridPosition(3, 3), inventory[0].Position);

        inventory.Dispose();
    }

    [Test]
    public void TryMoveItem_MovesSquareItem()
    {
        var inventory = new Inventory(5, 5, Allocator.Temp);
        var item = inventory.TryPlaceItem(_squareItem2x2, RotationDegree.None, new GridPosition(0, 0));

        Assert.IsTrue(item.IsValid);
        var result = inventory.TryMoveItem(item.InstanceId, new GridPosition(2, 2));

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
        var item1 = inventory.TryPlaceItem(_singleCellItem, RotationDegree.None, new GridPosition(0, 0));
        var item2 = inventory.TryPlaceItem(_singleCellItem, RotationDegree.None, new GridPosition(2, 0));

        Assert.IsTrue(item1.IsValid);
        Assert.IsTrue(item2.IsValid);

        var result = inventory.TryMoveItem(item1.InstanceId, new GridPosition(2, 0)); // Try to move to item2's position

        Assert.IsFalse(result);
        Assert.AreEqual(0, inventory.Grid[0, 0]); // Item1 didn't move
        Assert.AreEqual(1, inventory.Grid[2, 0]); // Item2 still there

        inventory.Dispose();
    }

    [Test]
    public void TryMoveItem_CanMoveToOverlapOwnPosition()
    {
        var inventory = new Inventory(5, 5, Allocator.Temp);
        var item = inventory.TryPlaceItem(_squareItem2x2, RotationDegree.None, new GridPosition(1, 1));

        Assert.IsTrue(item.IsValid);
        var result = inventory.TryMoveItem(item.InstanceId, new GridPosition(2, 1)); // Partial overlap with self

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
        var item = inventory.TryPlaceItem(_squareItem2x2, RotationDegree.None, new GridPosition(0, 0));

        Assert.IsTrue(item.IsValid);
        var result = inventory.TryMoveItem(item.InstanceId, new GridPosition(4, 4));

        Assert.IsFalse(result);
        Assert.AreEqual(new GridPosition(0, 0), inventory[0].Position);

        inventory.Dispose();
    }

    #endregion

    #region Query Methods Tests

    [Test]
    public void GetItemAt_ReturnsCorrectItemIndex()
    {
        var inventory = new Inventory(5, 5, Allocator.Temp);
        var item1 = inventory.TryPlaceItem(_singleCellItem, RotationDegree.None, new GridPosition(0, 0));
        var item2 = inventory.TryPlaceItem(_squareItem2x2, RotationDegree.None, new GridPosition(2, 2));

        Assert.IsTrue(item1.IsValid);
        Assert.IsTrue(item2.IsValid);

        Assert.AreEqual(item1.InstanceId, inventory.GetItemAt(new GridPosition(0, 0)).InstanceId);
        Assert.AreEqual(0, (int)item1.InstanceId.Id); // Instance ID is the index
        Assert.AreEqual(item2.InstanceId, inventory.GetItemAt(new GridPosition(2, 2)).InstanceId);
        Assert.AreEqual(item2.InstanceId, inventory.GetItemAt(new GridPosition(3, 3)).InstanceId);
        Assert.AreEqual(1, (int)item2.InstanceId.Id); // Instance ID is the index
        Assert.AreEqual(InventoryItemInstanceId.Invalid, inventory.GetItemAt(new GridPosition(1, 1)).InstanceId);

        inventory.Dispose();
    }

    [Test]
    public void GetItemAt_ReturnsMinusOneForEmptyCell()
    {
        var inventory = new Inventory(5, 5, Allocator.Temp);

        Assert.AreEqual(InventoryItemInstanceId.Invalid, inventory.GetItemAt(new GridPosition(0, 0)).InstanceId);
        Assert.AreEqual(InventoryItemInstanceId.Invalid, inventory.GetItemAt(new GridPosition(2, 2)).InstanceId);

        inventory.Dispose();
    }

    [Test]
    public void GetItemAt_ReturnsMinusOneForOutOfBounds()
    {
        var inventory = new Inventory(5, 5, Allocator.Temp);

        Assert.AreEqual(InventoryItemInstanceId.Invalid, inventory.GetItemAt(new GridPosition(-1, 0)).InstanceId);
        Assert.AreEqual(InventoryItemInstanceId.Invalid, inventory.GetItemAt(new GridPosition(5, 0)).InstanceId);
        Assert.AreEqual(InventoryItemInstanceId.Invalid, inventory.GetItemAt(new GridPosition(0, 5)).InstanceId);

        inventory.Dispose();
    }

    [Test]
    public void IsPositionOccupied_WorksCorrectly()
    {
        var inventory = new Inventory(5, 5, Allocator.Temp);

        Assert.IsFalse(inventory.IsPositionOccupied(new GridPosition(2, 2)));

        var item = inventory.TryPlaceItem(_singleCellItem, RotationDegree.None, new GridPosition(2, 2));
        Assert.IsTrue(item.IsValid);

        Assert.IsTrue(inventory.IsPositionOccupied(new GridPosition(2, 2)));
        Assert.IsFalse(inventory.IsPositionOccupied(new GridPosition(0, 0)));

        inventory.Dispose();
    }

    [Test]
    public void ContainsItem_FindsItemByInstanceId()
    {
        var inventory = new Inventory(5, 5, Allocator.Temp);
        var item1 = inventory.TryPlaceItem(_singleCellItem, RotationDegree.None, new GridPosition(0, 0));
        var item2 = inventory.TryPlaceItem(_singleCellItem, RotationDegree.None, new GridPosition(2, 0));

        Assert.IsTrue(item1.IsValid);
        Assert.IsTrue(item2.IsValid);

        Assert.IsTrue(inventory.ContainsItem(item1.InstanceId));
        Assert.IsTrue(inventory.ContainsItem(item2.InstanceId));
        Assert.IsFalse(inventory.ContainsItem((InventoryItemInstanceId)300));

        inventory.Dispose();
    }

    [Test]
    public void FindItemIndex_ReturnsCorrectIndex()
    {
        var inventory = new Inventory(5, 5, Allocator.Temp);
        var item1 = inventory.TryPlaceItem(_singleCellItem, RotationDegree.None, new GridPosition(0, 0));
        var item2 = inventory.TryPlaceItem(_singleCellItem, RotationDegree.None, new GridPosition(2, 0));

        Assert.IsTrue(item1.IsValid);
        Assert.IsTrue(item2.IsValid);

        Assert.AreEqual(0, (int)item1.InstanceId.Id);
        Assert.AreEqual(1, (int)item2.InstanceId.Id);

        inventory.Dispose();
    }

    [Test]
    public void TryGetItem_RetrievesItemSuccessfully()
    {
        var inventory = new Inventory(5, 5, Allocator.Temp);
        var item = inventory.TryPlaceItem(_singleCellItem, RotationDegree.None, new GridPosition(2, 2));

        Assert.IsTrue(item.IsValid);

        var retrievedItem = inventory[0];
        Assert.AreEqual(item.InstanceId, retrievedItem.InstanceId);
        Assert.AreEqual(new GridPosition(2, 2), retrievedItem.Position);

        inventory.Dispose();
    }

    [Test]
    public void CanPlaceItemAt_ValidatesPlacement()
    {
        var inventory = new Inventory(5, 5, Allocator.Temp);
        var item1 = inventory.TryPlaceItem(_squareItem2x2, RotationDegree.None, new GridPosition(1, 1));

        Assert.IsTrue(item1.IsValid);

        Assert.IsTrue(inventory.CanPlaceShapeAt(_singleCellItem.Shape, new GridPosition(0, 0)));
        Assert.IsFalse(inventory.CanPlaceShapeAt(_squareItem2x2.Shape, new GridPosition(2, 2))); // Overlaps
        Assert.IsFalse(inventory.CanPlaceShapeAt(_squareItem2x2.Shape, new GridPosition(4, 4))); // Out of bounds
        Assert.IsTrue(inventory.CanPlaceShapeAt(_squareItem2x2.Shape, new GridPosition(3, 0))); // Valid position

        inventory.Dispose();
    }

    [Test]
    public void GetFreeSpaceCount_CountsCorrectly()
    {
        var inventory = new Inventory(5, 5, Allocator.Temp);

        Assert.AreEqual(25, inventory.FreeSpaceCount);

        var item1 = inventory.TryPlaceItem(_singleCellItem, RotationDegree.None, new GridPosition(0, 0));
        Assert.IsTrue(item1.IsValid);
        Assert.AreEqual(24, inventory.FreeSpaceCount);

        var item2 = inventory.TryPlaceItem(_squareItem2x2, RotationDegree.None, new GridPosition(2, 2));
        Assert.IsTrue(item2.IsValid);
        Assert.AreEqual(20, inventory.FreeSpaceCount);

        inventory.Dispose();
    }

    [Test]
    public void GetOccupiedSpaceCount_CountsCorrectly()
    {
        var inventory = new Inventory(5, 5, Allocator.Temp);

        Assert.AreEqual(0, inventory.OccupiedSpaceCount);

        var item1 = inventory.TryPlaceItem(_lShapeItem, RotationDegree.None, new GridPosition(0, 0));
        Assert.IsTrue(item1.IsValid);
        Assert.AreEqual(3, inventory.OccupiedSpaceCount); // L-shape occupies 3 cells

        inventory.Dispose();
    }

    #endregion

    #region Auto-Placement Tests

    [Test]
    public void TryFindFirstFitPosition_FindsValidPosition()
    {
        var inventory = new Inventory(5, 5, Allocator.Temp);
        var item = inventory.TryPlaceItem(_singleCellItem, RotationDegree.None, new GridPosition(0, 0));

        Assert.IsTrue(item.IsValid);

        var position = inventory.FindFirstFitPosition(_squareItem2x2.Shape);

        Assert.IsTrue(position.IsValid);
        Assert.AreEqual(new GridPosition(1, 0), position); // First valid position

        inventory.Dispose();
    }

    [Test]
    public void TryFindFirstFitPosition_FailsWhenNoSpace()
    {
        var inventory = new Inventory(2, 2, Allocator.Temp);
        var item = inventory.TryPlaceItem(_squareItem2x2, RotationDegree.None, new GridPosition(0, 0));

        Assert.IsTrue(item.IsValid);

        var position = inventory.FindFirstFitPosition(_singleCellItem.Shape);

        Assert.IsFalse(position.IsValid);
        Assert.AreEqual(GridPosition.Invalid, position);

        inventory.Dispose();
    }

    [Test]
    public void TryAutoPlaceItem_PlacesItemAutomatically()
    {
        var inventory = new Inventory(5, 5, Allocator.Temp);
        var item1 = inventory.TryPlaceItem(_squareItem2x2, RotationDegree.None, new GridPosition(0, 0));

        Assert.IsTrue(item1.IsValid);

        var result = inventory.TryAutoPlaceItem(_squareItem2x2);

        Assert.IsTrue(result.IsValid);
        Assert.AreEqual(new GridPosition(2, 0), result.Position);
        Assert.AreEqual(2, inventory.ItemCount);
        Assert.AreEqual(result.Position, inventory[(int)result.InstanceId.Id].Position);

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
                var item = inventory.TryPlaceItem(_singleCellItem, RotationDegree.None, new GridPosition(x, y));
                Assert.IsTrue(item.IsValid);
            }
        }

        var result = inventory.TryAutoPlaceItem(_singleCellItem);

        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(InventoryItem.Invalid, result);
        Assert.AreEqual(9, inventory.ItemCount);

        inventory.Dispose();
    }

    #endregion

    #region Clear and Dispose Tests

    [Test]
    public void Clear_RemovesAllItems()
    {
        var inventory = new Inventory(5, 5, Allocator.Temp);
        var item1 = inventory.TryPlaceItem(_singleCellItem, RotationDegree.None, new GridPosition(0, 0));
        var item2 = inventory.TryPlaceItem(_squareItem2x2, RotationDegree.None, new GridPosition(2, 2));

        Assert.IsTrue(item1.IsValid);
        Assert.IsTrue(item2.IsValid);

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
        var item1 = inventory.TryPlaceItem(_squareItem2x2, RotationDegree.None, new GridPosition(0, 0));

        Assert.IsTrue(item1.IsValid);
        inventory.Clear();

        // Should be able to place item in same position after clear
        var result = inventory.TryPlaceItem(_squareItem2x2, RotationDegree.None, new GridPosition(0, 0));

        Assert.IsTrue(result.IsValid);
        Assert.AreEqual(1, inventory.ItemCount);

        inventory.Dispose();
    }

    [Test]
    public void Dispose_ReleasesResources()
    {
        var inventory = new Inventory(5, 5, Allocator.Temp);
        var item = inventory.TryPlaceItem(_singleCellItem, RotationDegree.None, new GridPosition(0, 0));

        Assert.IsTrue(item.IsValid);
        inventory.Dispose();

        // Test passes if no exceptions are thrown during dispose
        Assert.Pass();
    }

    [Test]
    public void DisposeWithJobHandle_CompletesSuccessfully()
    {
        var inventory = new Inventory(5, 5, Allocator.TempJob);
        var item = inventory.TryPlaceItem(_singleCellItem, RotationDegree.None, new GridPosition(0, 0));

        Assert.IsTrue(item.IsValid);

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
        var horizontalLine = inventory.TryPlaceItem(_lineItem3x1, RotationDegree.None, new GridPosition(1, 1));
        Assert.IsTrue(horizontalLine.IsValid);
        Assert.AreEqual(0, inventory.Grid[1, 1]);
        Assert.AreEqual(0, inventory.Grid[2, 1]);
        Assert.AreEqual(0, inventory.Grid[3, 1]);

        // Vertical line (1x3) - rotated 90 degrees
        var verticalLine = inventory.TryPlaceItem(_lineItem3x1, RotationDegree.Clockwise90, new GridPosition(0, 2));
        Assert.IsTrue(verticalLine.IsValid);
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
        var lShape0 = inventory.TryPlaceItem(_lShapeItem, RotationDegree.None, new GridPosition(0, 0));
        Assert.IsTrue(lShape0.IsValid);

        // Rotated 90 degrees
        var lShape90 = inventory.TryPlaceItem(_lShapeItem, RotationDegree.Clockwise90, new GridPosition(3, 0));
        Assert.IsTrue(lShape90.IsValid);

        // Rotated 180 degrees
        var lShape180 = inventory.TryPlaceItem(_lShapeItem, RotationDegree.Clockwise180, new GridPosition(0, 3));
        Assert.IsTrue(lShape180.IsValid);

        // Rotated 270 degrees
        var lShape270 = inventory.TryPlaceItem(_lShapeItem, RotationDegree.Clockwise270, new GridPosition(3, 3));
        Assert.IsTrue(lShape270.IsValid);

        Assert.AreEqual(4, inventory.ItemCount);

        inventory.Dispose();
    }

    [Test]
    public void TryPlaceItem_RotationAffectsBounds()
    {
        var inventory = new Inventory(3, 3, Allocator.Temp);

        // 3x1 line fits horizontally
        var horizontalLine = inventory.TryPlaceItem(_lineItem3x1, RotationDegree.None, new GridPosition(0, 0));
        Assert.IsTrue(horizontalLine.IsValid);
        inventory.Clear();

        // 3x1 line rotated 90 degrees (becomes 1x3) fits vertically
        var verticalLine = inventory.TryPlaceItem(_lineItem3x1, RotationDegree.Clockwise90, new GridPosition(0, 0));
        Assert.IsTrue(verticalLine.IsValid);
        inventory.Clear();

        // 3x1 line at bottom-right doesn't fit
        var lineOutOfBounds = inventory.TryPlaceItem(_lineItem3x1, RotationDegree.None, new GridPosition(1, 2));
        Assert.IsFalse(lineOutOfBounds.IsValid);

        inventory.Dispose();
    }

    [Test]
    public void TryMoveItem_WorksWithRotatedShapes()
    {
        var inventory = new Inventory(5, 5, Allocator.Temp);

        // Place a rotated T-shape
        var tShape = inventory.TryPlaceItem(_tShapeItem, RotationDegree.Clockwise90, new GridPosition(0, 0));
        Assert.IsTrue(tShape.IsValid);

        // Move it to a new position
        Assert.IsTrue(inventory.TryMoveItem(tShape.InstanceId, new GridPosition(3, 1)));

        // Verify the shape is at the new position with correct rotation
        Assert.AreEqual(new GridPosition(3, 1), inventory[(int)tShape.InstanceId.Id].Position);
        Assert.AreEqual(RotationDegree.Clockwise90, inventory[(int)tShape.InstanceId.Id].Rotation);

        inventory.Dispose();
    }

    #endregion

    #region Edge Cases and Boundary Tests

    [Test]
    public void TryPlaceItem_AtExactBoundary()
    {
        var inventory = new Inventory(3, 3, Allocator.Temp);

        // Single cell at each corner
        var topLeft = inventory.TryPlaceItem(_singleCellItem, RotationDegree.None, new GridPosition(0, 0));
        var topRight = inventory.TryPlaceItem(_singleCellItem, RotationDegree.None, new GridPosition(2, 0));
        var bottomLeft = inventory.TryPlaceItem(_singleCellItem, RotationDegree.None, new GridPosition(0, 2));
        var bottomRight = inventory.TryPlaceItem(_singleCellItem, RotationDegree.None, new GridPosition(2, 2));

        Assert.IsTrue(topLeft.IsValid);
        Assert.IsTrue(topRight.IsValid);
        Assert.IsTrue(bottomLeft.IsValid);
        Assert.IsTrue(bottomRight.IsValid);

        Assert.AreEqual(4, inventory.ItemCount);

        inventory.Dispose();
    }

    [Test]
    public void RemoveItem_LastItemDoesNotTriggerIndexUpdate()
    {
        var inventory = new Inventory(5, 5, Allocator.Temp);
        var item1 = inventory.TryPlaceItem(_singleCellItem, RotationDegree.None, new GridPosition(0, 0));
        var item2 = inventory.TryPlaceItem(_singleCellItem, RotationDegree.None, new GridPosition(2, 0));

        Assert.IsTrue(item1.IsValid);
        Assert.IsTrue(item2.IsValid);

        // Remove last item
        inventory.RemoveItem(item2.InstanceId);

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
        var item1 = inventory.TryPlaceItem(_squareItem2x2, RotationDegree.None, new GridPosition(0, 0));
        var item2 = inventory.TryPlaceItem(_lShapeItem, RotationDegree.Clockwise90, new GridPosition(3, 0));
        var item3 = inventory.TryPlaceItem(_tShapeItem, RotationDegree.None, new GridPosition(0, 3));
        var item4 = inventory.TryPlaceItem(_lineItem3x1, RotationDegree.Clockwise90, new GridPosition(5, 0));

        Assert.IsTrue(item1.IsValid);
        Assert.IsTrue(item2.IsValid);
        Assert.IsTrue(item3.IsValid);
        Assert.IsTrue(item4.IsValid);

        // Move item2
        Assert.IsTrue(inventory.TryMoveItem(item2.InstanceId, new GridPosition(3, 3)));

        // Remove item1
        Assert.IsTrue(inventory.RemoveItem(item1.InstanceId));

        // Should have 3 items remaining (indices are stable, so indices don't change)
        Assert.AreEqual(3, inventory.ItemCount);

        // Try auto-place a new item
        var autoItem = inventory.TryAutoPlaceItem(_squareItem2x2);
        Assert.IsTrue(autoItem.IsValid);
        Assert.AreEqual(new GridPosition(0, 0), autoItem.Position); // Should place where item1 was

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

        var item = inventory.TryPlaceItem(_singleCellItem, RotationDegree.None, new GridPosition(0, 0));
        Assert.IsFalse(item.IsValid);
        Assert.IsFalse(inventory.FindFirstFitPosition(_singleCellItem.Shape).IsValid);

        inventory.Dispose();
    }

    #endregion
}
