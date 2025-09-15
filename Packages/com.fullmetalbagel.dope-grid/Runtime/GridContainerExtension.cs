using Unity.Collections;
using Unity.Mathematics;

namespace DopeGrid;

public static class GridContainerExtension
{
    public static int2 FindFirstFit(this GridShape2D inventory, GridShape2D item)
    {
        var maxY = inventory.Height - item.Height + 1;
        var maxX = inventory.Width - item.Width + 1;

        for (var y = 0; y < maxY; y++)
        for (var x = 0; x < maxX; x++)
            if (inventory.CanPlaceItem(item, new int2(x, y)))
                return new int2(x, y);

        return new int2(-1, -1);
    }

    public static int2 FindBestFit(this GridShape2D inventory, GridShape2D item)
    {
        var bestPos = new int2(-1, -1);
        var bestScore = int.MaxValue;

        var maxY = inventory.Height - item.Height + 1;
        var maxX = inventory.Width - item.Width + 1;

        for (var y = 0; y < maxY; y++)
        for (var x = 0; x < maxX; x++)
            if (inventory.CanPlaceItem(item, new int2(x, y)))
            {
                // Score based on position (prefer top-left)
                var score = y * inventory.Width + x;
                if (score < bestScore)
                {
                    bestScore = score;
                    bestPos = new int2(x, y);
                }
            }

        return bestPos;
    }

    public static int PlaceMultipleShapes(
        this GridShape2D inventory,
        NativeArray<GridShape2D> items,
        NativeArray<int2> outPositions)
    {
        var placed = 0;

        for (var i = 0; i < items.Length; i++)
        {
            var pos = inventory.FindFirstFit(items[i]);
            if (pos.x >= 0)
            {
                inventory.PlaceItem(items[i], pos);
                outPositions[placed] = pos;
                placed++;
            }
        }

        return placed;
    }

    public static bool CanPlaceItem(this GridShape2D inventory, GridShape2D item, int2 pos)
    {
        // Bounds check
        if (pos.x < 0 || pos.y < 0 || pos.x + item.Width > inventory.Width || pos.y + item.Height > inventory.Height)
            return false;

        // Check each bit of the shape against the grid
        for (var sy = 0; sy < item.Height; sy++)
        for (var sx = 0; sx < item.Width; sx++)
        {
            var shapePos = new int2(sx, sy);
            if (item.GetCell(shapePos))
            {
                var gridPos = new int2(pos.x + sx, pos.y + sy);
                if (inventory.GetCell(gridPos))
                    return false;
            }
        }

        return true;
    }

    public static void PlaceItem(this GridShape2D inventory, GridShape2D item, int2 pos)
    {
        for (var sy = 0; sy < item.Height; sy++)
        for (var sx = 0; sx < item.Width; sx++)
        {
            var shapePos = new int2(sx, sy);
            if (item.GetCell(shapePos))
            {
                var gridPos = new int2(pos.x + sx, pos.y + sy);
                inventory.SetCell(gridPos, true);
            }
        }
    }

    public static void RemoveItem(this GridShape2D inventory, GridShape2D item, int2 pos)
    {
        for (var sy = 0; sy < item.Height; sy++)
        for (var sx = 0; sx < item.Width; sx++)
        {
            var shapePos = new int2(sx, sy);
            if (item.GetCell(shapePos))
            {
                var gridPos = new int2(pos.x + sx, pos.y + sy);
                inventory.SetCell(gridPos, false);
            }
        }
    }
}
