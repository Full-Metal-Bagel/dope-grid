using System;
using NUnit.Framework;

namespace DopeGrid.Inventory.Tests
{
    [TestFixture]
    public class DefaultGameInventoryTests
    {
        private SharedInventoryData _sharedData = null!;
        private DefaultGameInventory _inventory = null!;
        private const int Width = 10;
        private const int Height = 10;

        [SetUp]
        public void SetUp()
        {
            _sharedData = new SharedInventoryData();
            _inventory = new DefaultGameInventory(Width, Height, _sharedData);
        }

        [TearDown]
        public void TearDown()
        {
            _inventory?.Dispose();
        }

        #region Basic Properties

        [Test]
        public void Constructor_InitializesWithCorrectDimensions()
        {
            Assert.AreEqual(Width, _inventory.Width);
            Assert.AreEqual(Height, _inventory.Height);
        }

        [Test]
        public void Indexer_ReturnsMinusOneForEmptyCells()
        {
            Assert.AreEqual(-1, _inventory[0, 0]);
            Assert.AreEqual(-1, _inventory[5, 5]);
        }

        [Test]
        public void IsOccupied_ReturnsFalseForEmptyCells()
        {
            Assert.IsFalse(_inventory.IsOccupied(0, 0));
            Assert.IsFalse(_inventory.IsOccupied(5, 5));
        }

        #endregion

        #region Item Addition and Removal

        [Test]
        public void TryMoveItem_AddsNewItemSuccessfully()
        {
            var itemId = Guid.NewGuid();
            var shape = CreateSquareShape(2, 2);
            _sharedData.ItemShapeMap[itemId] = shape;
            _sharedData.ItemRotationMap[itemId] = RotationDegree.Clockwise0;

            var result = _inventory.TryMoveItem(itemId);

            Assert.IsTrue(result);
            var item = _inventory.GetItem(itemId);
            Assert.IsTrue(item.IsValid);
            Assert.AreEqual(0, item.X);
            Assert.AreEqual(0, item.Y);
        }

        [Test]
        public void TryMoveItem_WithSpecificPosition_PlacesItemCorrectly()
        {
            var itemId = Guid.NewGuid();
            var shape = CreateSquareShape(2, 2);
            _sharedData.ItemShapeMap[itemId] = shape;
            _sharedData.ItemRotationMap[itemId] = RotationDegree.Clockwise0;

            var result = _inventory.TryMoveItem(itemId, 3, 4, RotationDegree.Clockwise0);

            Assert.IsTrue(result);
            var item = _inventory.GetItem(itemId);
            Assert.AreEqual(3, item.X);
            Assert.AreEqual(4, item.Y);
        }

        [Test]
        public void TryMoveItem_WithRotation_PlacesItemWithCorrectRotation()
        {
            var itemId = Guid.NewGuid();
            var shape = CreateLShape();
            _sharedData.ItemShapeMap[itemId] = shape;
            _sharedData.ItemRotationMap[itemId] = RotationDegree.Clockwise0;

            var result = _inventory.TryMoveItem(itemId, 2, 2, RotationDegree.Clockwise90);

            Assert.IsTrue(result);
            Assert.AreEqual(RotationDegree.Clockwise90, _sharedData.GetRotation(itemId));
        }

        [Test]
        public void TryMoveItem_FailsWhenPositionOccupied()
        {
            var item1 = Guid.NewGuid();
            var item2 = Guid.NewGuid();
            var shape = CreateSquareShape(2, 2);

            _sharedData.ItemShapeMap[item1] = shape;
            _sharedData.ItemShapeMap[item2] = shape;
            _sharedData.ItemRotationMap[item1] = RotationDegree.Clockwise0;
            _sharedData.ItemRotationMap[item2] = RotationDegree.Clockwise0;

            _inventory.TryMoveItem(item1, 0, 0, RotationDegree.Clockwise0);
            var result = _inventory.TryMoveItem(item2, 0, 0, RotationDegree.Clockwise0);

            Assert.IsFalse(result);
        }

        [Test]
        public void TryMoveItem_FailsWhenOutOfBounds()
        {
            var itemId = Guid.NewGuid();
            var shape = CreateSquareShape(2, 2);
            _sharedData.ItemShapeMap[itemId] = shape;
            _sharedData.ItemRotationMap[itemId] = RotationDegree.Clockwise0;

            var result = _inventory.TryMoveItem(itemId, Width - 1, Height - 1, RotationDegree.Clockwise0);

            Assert.IsFalse(result);
        }

        [Test]
        public void TryRemoveItem_RemovesExistingItem()
        {
            var itemId = Guid.NewGuid();
            var shape = CreateSquareShape(2, 2);
            _sharedData.ItemShapeMap[itemId] = shape;
            _sharedData.ItemRotationMap[itemId] = RotationDegree.Clockwise0;

            _inventory.TryMoveItem(itemId, 0, 0, RotationDegree.Clockwise0);
            var result = _inventory.TryRemoveItem(itemId);

            Assert.IsTrue(result);
            var item = _inventory.GetItem(itemId);
            Assert.IsTrue(item.IsInvalid);
        }

        [Test]
        public void TryRemoveItem_ReturnsFalseForNonExistentItem()
        {
            var itemId = Guid.NewGuid();
            var result = _inventory.TryRemoveItem(itemId);

            Assert.IsFalse(result);
        }

        [Test]
        public void TryRemoveItem_FreesOccupiedCells()
        {
            var itemId = Guid.NewGuid();
            var shape = CreateSquareShape(2, 2);
            _sharedData.ItemShapeMap[itemId] = shape;
            _sharedData.ItemRotationMap[itemId] = RotationDegree.Clockwise0;

            _inventory.TryMoveItem(itemId, 0, 0, RotationDegree.Clockwise0);
            _inventory.TryRemoveItem(itemId);

            Assert.IsFalse(_inventory.IsOccupied(0, 0));
            Assert.IsFalse(_inventory.IsOccupied(1, 1));
        }

        #endregion

        #region Item Movement Within Inventory

        [Test]
        public void TryMoveItem_MovesExistingItemToNewPosition()
        {
            var itemId = Guid.NewGuid();
            var shape = CreateSquareShape(2, 2);
            _sharedData.ItemShapeMap[itemId] = shape;
            _sharedData.ItemRotationMap[itemId] = RotationDegree.Clockwise0;

            _inventory.TryMoveItem(itemId, 0, 0, RotationDegree.Clockwise0);
            var result = _inventory.TryMoveItem(itemId, 3, 3, RotationDegree.Clockwise0);

            Assert.IsTrue(result);
            var item = _inventory.GetItem(itemId);
            Assert.AreEqual(3, item.X);
            Assert.AreEqual(3, item.Y);
            Assert.IsFalse(_inventory.IsOccupied(0, 0));
            Assert.IsTrue(_inventory.IsOccupied(3, 3));
        }

        [Test]
        public void TryMoveItem_ReturnsFalseWhenMovingToSamePosition()
        {
            var itemId = Guid.NewGuid();
            var shape = CreateSquareShape(2, 2);
            _sharedData.ItemShapeMap[itemId] = shape;
            _sharedData.ItemRotationMap[itemId] = RotationDegree.Clockwise0;

            _inventory.TryMoveItem(itemId, 2, 2, RotationDegree.Clockwise0);
            var result = _inventory.TryMoveItem(itemId, 2, 2, RotationDegree.Clockwise0);

            Assert.IsFalse(result);
        }

        [Test]
        public void TryMoveItem_CanRotateItemInPlace()
        {
            var itemId = Guid.NewGuid();
            var shape = CreateLShape();
            _sharedData.ItemShapeMap[itemId] = shape;
            _sharedData.ItemRotationMap[itemId] = RotationDegree.Clockwise0;

            _inventory.TryMoveItem(itemId, 0, 0, RotationDegree.Clockwise0);
            var result = _inventory.TryMoveItem(itemId, 0, 0, RotationDegree.Clockwise90);

            Assert.IsTrue(result);
            Assert.AreEqual(RotationDegree.Clockwise90, _sharedData.GetRotation(itemId));
        }

        [Test]
        public void TryMoveItem_WithRotationOnly_FindsFirstFit()
        {
            var itemId = Guid.NewGuid();
            var shape = CreateLShape();
            _sharedData.ItemShapeMap[itemId] = shape;
            _sharedData.ItemRotationMap[itemId] = RotationDegree.Clockwise0;

            var result = _inventory.TryMoveItem(itemId, RotationDegree.Clockwise90);

            Assert.IsTrue(result);
            Assert.AreEqual(RotationDegree.Clockwise90, _sharedData.GetRotation(itemId));
        }

        #endregion

        #region CanMoveItem Tests

        [Test]
        public void CanMoveItem_ReturnsTrueForValidPlacement()
        {
            var itemId = Guid.NewGuid();
            var shape = CreateSquareShape(2, 2);
            _sharedData.ItemShapeMap[itemId] = shape;
            _sharedData.ItemRotationMap[itemId] = RotationDegree.Clockwise0;

            var canMove = _inventory.CanMoveItem(itemId, 0, 0, RotationDegree.Clockwise0);

            Assert.IsTrue(canMove);
        }

        [Test]
        public void CanMoveItem_ReturnsFalseForOccupiedSpace()
        {
            var item1 = Guid.NewGuid();
            var item2 = Guid.NewGuid();
            var shape = CreateSquareShape(2, 2);

            _sharedData.ItemShapeMap[item1] = shape;
            _sharedData.ItemShapeMap[item2] = shape;
            _sharedData.ItemRotationMap[item1] = RotationDegree.Clockwise0;
            _sharedData.ItemRotationMap[item2] = RotationDegree.Clockwise0;

            _inventory.TryMoveItem(item1, 0, 0, RotationDegree.Clockwise0);
            var canMove = _inventory.CanMoveItem(item2, 0, 0, RotationDegree.Clockwise0);

            Assert.IsFalse(canMove);
        }

        [Test]
        public void CanMoveItem_ReturnsTrueWhenMovingItemToOverlapItsOwnSpace()
        {
            var itemId = Guid.NewGuid();
            var shape = CreateSquareShape(3, 3);
            _sharedData.ItemShapeMap[itemId] = shape;
            _sharedData.ItemRotationMap[itemId] = RotationDegree.Clockwise0;

            _inventory.TryMoveItem(itemId, 0, 0, RotationDegree.Clockwise0);
            var canMove = _inventory.CanMoveItem(itemId, 1, 1, RotationDegree.Clockwise0);

            Assert.IsTrue(canMove);
        }

        #endregion

        #region Enumeration Tests

        [Test]
        public void GetEnumerator_IteratesOverAllItems()
        {
            var item1 = Guid.NewGuid();
            var item2 = Guid.NewGuid();
            var shape = CreateSquareShape(2, 2);

            _sharedData.ItemShapeMap[item1] = shape;
            _sharedData.ItemShapeMap[item2] = shape;
            _sharedData.ItemRotationMap[item1] = RotationDegree.Clockwise0;
            _sharedData.ItemRotationMap[item2] = RotationDegree.Clockwise0;

            _inventory.TryMoveItem(item1, 0, 0, RotationDegree.Clockwise0);
            _inventory.TryMoveItem(item2, 3, 3, RotationDegree.Clockwise0);

            var items = EnumerateItems(_inventory);

            Assert.AreEqual(2, items.Count);
            Assert.Contains(item1, items);
            Assert.Contains(item2, items);
        }

        [Test]
        public void GetEnumerator_ReturnsEmptyForEmptyInventory()
        {
            var items = EnumerateItems(_inventory);
            Assert.AreEqual(0, items.Count);
        }

        #endregion

        #region Item ID Mapping Tests

        [Test]
        public void GetItemInstanceId_ReturnsCorrectId()
        {
            var itemId = Guid.NewGuid();
            var shape = CreateSquareShape(2, 2);
            _sharedData.ItemShapeMap[itemId] = shape;
            _sharedData.ItemRotationMap[itemId] = RotationDegree.Clockwise0;

            _inventory.TryMoveItem(itemId, 0, 0, RotationDegree.Clockwise0);
            var item = _inventory.GetItem(itemId);
            var retrievedId = _inventory.GetItemInstanceId(item.Id);

            Assert.AreEqual(itemId, retrievedId);
        }

        [Test]
        public void GetItem_ReturnsInvalidForNonExistentItem()
        {
            var itemId = Guid.NewGuid();
            var item = _inventory.GetItem(itemId);

            Assert.IsTrue(item.IsInvalid);
        }

        [Test]
        public void GetItemById_ReturnsCorrectItem()
        {
            var itemId = Guid.NewGuid();
            var shape = CreateSquareShape(2, 2);
            _sharedData.ItemShapeMap[itemId] = shape;
            _sharedData.ItemRotationMap[itemId] = RotationDegree.Clockwise0;

            _inventory.TryMoveItem(itemId, 3, 4, RotationDegree.Clockwise0);
            var item = _inventory.GetItem(itemId);
            var retrievedItem = _inventory.GetItemById(item.Id);

            Assert.AreEqual(item.X, retrievedItem.X);
            Assert.AreEqual(item.Y, retrievedItem.Y);
            Assert.AreEqual(item.Id, retrievedItem.Id);
        }

        #endregion

        #region Owner Management Tests

        [Test]
        public void TryMoveItem_SetsOwnerInSharedData()
        {
            var itemId = Guid.NewGuid();
            var shape = CreateSquareShape(2, 2);
            _sharedData.ItemShapeMap[itemId] = shape;
            _sharedData.ItemRotationMap[itemId] = RotationDegree.Clockwise0;

            _inventory.TryMoveItem(itemId, 0, 0, RotationDegree.Clockwise0);

            Assert.AreEqual(_inventory, _sharedData.GetOwner(itemId));
        }

        [Test]
        public void TryRemoveItem_ClearsOwnerInSharedData()
        {
            var itemId = Guid.NewGuid();
            var shape = CreateSquareShape(2, 2);
            _sharedData.ItemShapeMap[itemId] = shape;
            _sharedData.ItemRotationMap[itemId] = RotationDegree.Clockwise0;

            _inventory.TryMoveItem(itemId, 0, 0, RotationDegree.Clockwise0);
            _inventory.TryRemoveItem(itemId);

            Assert.IsNull(_sharedData.GetOwner(itemId));
        }

        [Test]
        public void TryMoveItem_RemovesItemFromPreviousOwner()
        {
            var inventory2 = new DefaultGameInventory(Width, Height, _sharedData);
            var itemId = Guid.NewGuid();
            var shape = CreateSquareShape(2, 2);
            _sharedData.ItemShapeMap[itemId] = shape;
            _sharedData.ItemRotationMap[itemId] = RotationDegree.Clockwise0;

            _inventory.TryMoveItem(itemId, 0, 0, RotationDegree.Clockwise0);
            inventory2.TryMoveItem(itemId, 0, 0, RotationDegree.Clockwise0);

            Assert.IsTrue(_inventory.GetItem(itemId).IsInvalid);
            Assert.IsTrue(inventory2.GetItem(itemId).IsValid);
            Assert.AreEqual(inventory2, _sharedData.GetOwner(itemId));

            inventory2.Dispose();
        }

        #endregion

        #region Edge Cases

        [Test]
        public void TryMoveItem_FailsWithEmptyGuid()
        {
            var shape = CreateSquareShape(2, 2);
            _sharedData.ItemShapeMap[Guid.Empty] = shape;
            _sharedData.ItemRotationMap[Guid.Empty] = RotationDegree.Clockwise0;

            var result = _inventory.TryMoveItem(Guid.Empty, 0, 0, RotationDegree.Clockwise0);

            Assert.IsFalse(result);
        }

        [Test]
        public void TryMoveItem_FailsWithNegativeCoordinates()
        {
            var itemId = Guid.NewGuid();
            var shape = CreateSquareShape(2, 2);
            _sharedData.ItemShapeMap[itemId] = shape;
            _sharedData.ItemRotationMap[itemId] = RotationDegree.Clockwise0;

            var result = _inventory.TryMoveItem(itemId, -1, 0, RotationDegree.Clockwise0);

            Assert.IsFalse(result);
        }

        [Test]
        public void TryMoveItem_FailsWithEmptyShape()
        {
            var itemId = Guid.NewGuid();
            _sharedData.ItemShapeMap[itemId] = ImmutableGridShape.Empty;
            _sharedData.ItemRotationMap[itemId] = RotationDegree.Clockwise0;

            var result = _inventory.TryMoveItem(itemId, 0, 0, RotationDegree.Clockwise0);

            Assert.IsFalse(result);
        }

        [Test]
        public void MultipleItems_CanCoexistWithoutOverlap()
        {
            var item1 = Guid.NewGuid();
            var item2 = Guid.NewGuid();
            var item3 = Guid.NewGuid();
            var shape = CreateSquareShape(2, 2);

            _sharedData.ItemShapeMap[item1] = shape;
            _sharedData.ItemShapeMap[item2] = shape;
            _sharedData.ItemShapeMap[item3] = shape;
            _sharedData.ItemRotationMap[item1] = RotationDegree.Clockwise0;
            _sharedData.ItemRotationMap[item2] = RotationDegree.Clockwise0;
            _sharedData.ItemRotationMap[item3] = RotationDegree.Clockwise0;

            var result1 = _inventory.TryMoveItem(item1, 0, 0, RotationDegree.Clockwise0);
            var result2 = _inventory.TryMoveItem(item2, 2, 0, RotationDegree.Clockwise0);
            var result3 = _inventory.TryMoveItem(item3, 0, 2, RotationDegree.Clockwise0);

            Assert.IsTrue(result1);
            Assert.IsTrue(result2);
            Assert.IsTrue(result3);

            var items = EnumerateItems(_inventory);
            Assert.AreEqual(3, items.Count);
        }

        #endregion

        #region Helper Methods

        private static ImmutableGridShape CreateSquareShape(int width, int height)
        {
            var shape = new FixedGridShape<ulong>(width, height);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    shape[x, y] = true;
                }
            }
            return shape.GetOrCreateImmutable();
        }

        private static ImmutableGridShape CreateLShape()
        {
            // Creates an L-shaped item:
            // X
            // X
            // XX
            var shape = new FixedGridShape<ulong>(2, 3);
            shape[0, 0] = true;
            shape[0, 1] = true;
            shape[0, 2] = true;
            shape[1, 2] = true;
            return shape.GetOrCreateImmutable();
        }

        private static System.Collections.Generic.List<Guid> EnumerateItems(IGameInventory inventory)
        {
            var result = new System.Collections.Generic.List<Guid>();
            var enumerator = inventory.GetEnumerator();
            while (enumerator.MoveNext())
            {
                result.Add(enumerator.Current);
            }
            return result;
        }

        #endregion
    }
}
