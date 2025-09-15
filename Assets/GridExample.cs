using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace DopeGrid
{
    public class GridExample : MonoBehaviour
    {
        private GridContainer _gridContainerSystem;

        private void Start()
        {
            // Create a 10x10 inventory grid
            _gridContainerSystem = new GridContainer(10, 10);

            // Add some items
            AddTestItems();

            // Test operations
            TestGridContainerOperations();
        }

        private void OnDestroy()
        {
            // Clean up native collections
            _gridContainerSystem.Dispose();
        }

        private void AddTestItems()
        {
            // Add a sword (3x1)
            using (var sword = Shapes.Line(3))
            {
                if (_gridContainerSystem.TryAddItem(sword))
                    Debug.Log("Added sword to inventory");
            }

            // Add a shield (2x2)
            using (var shield = Shapes.Square(2))
            {
                if (_gridContainerSystem.TryAddItem(shield))
                    Debug.Log("Added shield to inventory");
            }

            // Add an L-shaped item
            using (var lItem = Shapes.LShape())
            {
                if (_gridContainerSystem.TryAddItem(lItem))
                    Debug.Log("Added L-shaped item to inventory");
            }

            // Add a cross-shaped item
            using (var cross = Shapes.Cross())
            {
                if (_gridContainerSystem.TryAddItem(cross))
                    Debug.Log("Added cross to inventory");
            }
        }

        private void TestGridContainerOperations()
        {
            Debug.Log($"GridContainer has {_gridContainerSystem.ItemCount} items");
            Debug.Log($"Free space: {_gridContainerSystem.FreeSpace} cells");

            // Try to add item at specific position
            using (var smallItem = Shapes.Single())
            {
                if (_gridContainerSystem.TryAddItemAt(smallItem, new int2(9, 9)))
                    Debug.Log("Added item at specific position (9,9)");
            }

            // Test if position is occupied
            var occupied = _gridContainerSystem.IsCellOccupied(new int2(0, 0));
            Debug.Log($"Cell (0,0) is occupied: {occupied}");
        }

        // Example of using GridShape2D directly with InventoryOps
        public void DirectGridUsage()
        {
            using var grid = new GridShape2D(8, 8, Allocator.Temp);
            using (var shape1 = Shapes.Square(2))
            using (var shape2 = Shapes.Line(3))
            {
                // Find best position for first shape
                var pos1 = grid.FindBestFit(shape1);
                if (pos1.x >= 0)
                {
                    grid.PlaceItem(shape1, pos1);
                    Debug.Log($"Placed square at best position ({pos1.x}, {pos1.y})");
                }

                // Find first available position for second shape
                var pos2 = grid.FindFirstFit(shape2);
                if (pos2.x >= 0)
                {
                    grid.PlaceItem(shape2, pos2);
                    Debug.Log($"Placed line at first fit ({pos2.x}, {pos2.y})");
                }

                // Count free cells
                var freeCount = 0;
                for (var y = 0; y < grid.Height; y++)
                for (var x = 0; x < grid.Width; x++)
                    if (!grid.GetCell(new int2(x, y)))
                        freeCount++;

                Debug.Log($"Grid has {freeCount} free cells");
            }
        }
    }
}
