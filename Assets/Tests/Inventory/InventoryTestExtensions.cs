using DopeGrid;
using DopeGrid.Inventory;
using DopeGrid.Native;
using Unity.Mathematics;

namespace DopeGrid.Inventory
{
    public static class InventoryTestExtensions
    {
        public static bool TryFindFirstFitPosition(this ref Inventory inventory, ImmutableGridShape shape, out int2 position)
        {
            var pos = inventory.FindFirstFitPosition(shape);
            if (pos.IsValid)
            {
                position = new int2(pos.X, pos.Y);
                return true;
            }
            position = new int2(-1, -1);
            return false;
        }

        public static bool TryAutoPlaceItem(this ref Inventory inventory, InventoryItemInstanceId id, ItemDefinition itemDefinition, out int2 position)
        {
            var item = inventory.TryAutoPlaceItem(id, itemDefinition);
            if (item.IsValid)
            {
                position = new int2(item.Position.X, item.Position.Y);
                return true;
            }
            position = new int2(-1, -1);
            return false;
        }
    }
}

