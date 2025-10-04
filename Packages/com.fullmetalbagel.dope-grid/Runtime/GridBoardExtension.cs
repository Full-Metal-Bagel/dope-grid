using JetBrains.Annotations;
using Unity.Collections;

namespace DopeGrid.Native;

public static class GridBoardExtension
{
    [Pure, MustUseReturnValue]
    public static GridPosition FindFirstFit(this GridShape container, in GridShape.ReadOnly item)
    {
        return container.AsReadOnly().FindFirstFit(item);
    }

    [Pure, MustUseReturnValue]
    public static GridPosition FindFirstFit(this in GridShape.ReadOnly container, in GridShape.ReadOnly item)
    {
        var maxY = container.Height - item.Height + 1;
        var maxX = container.Width - item.Width + 1;

        for (var y = 0; y < maxY; y++)
        for (var x = 0; x < maxX; x++)
            if (container.CanPlaceItem(item, new GridPosition(x, y)))
                return new GridPosition(x, y);

        return new GridPosition(-1, -1);
    }

    public static int PlaceMultipleShapes(
        this GridShape container,
        NativeArray<GridShape> items,
        NativeArray<GridPosition> outPositions)
    {
        var placed = 0;

        var readonlyContainer = container.AsReadOnly();
        for (var i = 0; i < items.Length; i++)
        {
            var pos = readonlyContainer.FindFirstFit(items[i]);
            if (pos.X >= 0)
            {
                container.PlaceItem(items[i], pos);
                outPositions[placed] = pos;
                placed++;
            }
        }

        return placed;
    }

    [Pure, MustUseReturnValue]
    public static bool CanPlaceItem(this GridShape container, in GridShape.ReadOnly item, GridPosition pos)
    {
        return container.AsReadOnly().CanPlaceItem(item, pos);
    }

    [Pure, MustUseReturnValue]
    public static bool CanPlaceItem(this in GridShape.ReadOnly container, in GridShape.ReadOnly item, GridPosition pos)
    {
        // Bounds check
        if (pos.X < 0 || pos.Y < 0 || pos.X + item.Width > container.Width || pos.Y + item.Height > container.Height)
            return false;

        // Check each bit of the shape against the grid
        for (var sy = 0; sy < item.Height; sy++)
        for (var sx = 0; sx < item.Width; sx++)
        {
            var shapePos = new GridPosition(sx, sy);
            if (item.GetCell(shapePos))
            {
                var gridPos = new GridPosition(pos.X + sx, pos.Y + sy);
                if (container.GetCell(gridPos))
                    return false;
            }
        }

        return true;
    }

    public static void PlaceItem(this GridShape container, in GridShape.ReadOnly item, GridPosition pos)
    {
        for (var sy = 0; sy < item.Height; sy++)
        for (var sx = 0; sx < item.Width; sx++)
        {
            var shapePos = new GridPosition(sx, sy);
            if (item.GetCell(shapePos))
            {
                var gridPos = new GridPosition(pos.X + sx, pos.Y + sy);
                container.SetCell(gridPos, true);
            }
        }
    }

    public static void RemoveItem(this GridShape container, in GridShape.ReadOnly item, GridPosition pos)
    {
        for (var sy = 0; sy < item.Height; sy++)
        for (var sx = 0; sx < item.Width; sx++)
        {
            var shapePos = new GridPosition(sx, sy);
            if (item.GetCell(shapePos))
            {
                var gridPos = new GridPosition(pos.X + sx, pos.Y + sy);
                container.SetCell(gridPos, false);
            }
        }
    }
}
