using JetBrains.Annotations;
using Unity.Collections;
using Unity.Mathematics;

namespace DopeGrid.Native;

public static class GridBoardExtension
{
    [Pure, MustUseReturnValue]
    public static int2 FindFirstFit(this GridShape container, in GridShape.ReadOnly item)
    {
        return container.AsReadOnly().FindFirstFit(item);
    }

    [Pure, MustUseReturnValue]
    public static int2 FindFirstFit(this in GridShape.ReadOnly container, in GridShape.ReadOnly item)
    {
        var maxY = container.Height - item.Height + 1;
        var maxX = container.Width - item.Width + 1;

        for (var y = 0; y < maxY; y++)
        for (var x = 0; x < maxX; x++)
            if (container.CanPlaceItem(item, new int2(x, y)))
                return new int2(x, y);

        return new int2(-1, -1);
    }

    public static int PlaceMultipleShapes(
        this GridShape container,
        NativeArray<GridShape> items,
        NativeArray<int2> outPositions)
    {
        var placed = 0;

        var readonlyContainer = container.AsReadOnly();
        for (var i = 0; i < items.Length; i++)
        {
            var pos = readonlyContainer.FindFirstFit(items[i]);
            if (pos.x >= 0)
            {
                container.PlaceItem(items[i], pos);
                outPositions[placed] = pos;
                placed++;
            }
        }

        return placed;
    }

    [Pure, MustUseReturnValue]
    public static bool CanPlaceItem(this GridShape container, in GridShape.ReadOnly item, int2 pos)
    {
        return container.AsReadOnly().CanPlaceItem(item, pos);
    }

    [Pure, MustUseReturnValue]
    public static bool CanPlaceItem(this in GridShape.ReadOnly container, in GridShape.ReadOnly item, int2 pos)
    {
        // Bounds check
        if (pos.x < 0 || pos.y < 0 || pos.x + item.Width > container.Width || pos.y + item.Height > container.Height)
            return false;

        // Check each bit of the shape against the grid
        for (var sy = 0; sy < item.Height; sy++)
        for (var sx = 0; sx < item.Width; sx++)
        {
            var shapePos = new int2(sx, sy);
            if (item.GetCell(shapePos))
            {
                var gridPos = new int2(pos.x + sx, pos.y + sy);
                if (container.GetCell(gridPos))
                    return false;
            }
        }

        return true;
    }

    public static void PlaceItem(this GridShape container, in GridShape.ReadOnly item, int2 pos)
    {
        for (var sy = 0; sy < item.Height; sy++)
        for (var sx = 0; sx < item.Width; sx++)
        {
            var shapePos = new int2(sx, sy);
            if (item.GetCell(shapePos))
            {
                var gridPos = new int2(pos.x + sx, pos.y + sy);
                container.SetCell(gridPos, true);
            }
        }
    }

    public static void RemoveItem(this GridShape container, in GridShape.ReadOnly item, int2 pos)
    {
        for (var sy = 0; sy < item.Height; sy++)
        for (var sx = 0; sx < item.Width; sx++)
        {
            var shapePos = new int2(sx, sy);
            if (item.GetCell(shapePos))
            {
                var gridPos = new int2(pos.x + sx, pos.y + sy);
                container.SetCell(gridPos, false);
            }
        }
    }
}
