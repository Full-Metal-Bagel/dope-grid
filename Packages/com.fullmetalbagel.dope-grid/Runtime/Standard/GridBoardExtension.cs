using JetBrains.Annotations;

namespace DopeGrid.Standard;

public static class GridBoardExtension
{
    [Pure, MustUseReturnValue]
    public static (int x, int y) FindFirstFit(this GridShape container, in GridShape.ReadOnly item)
    {
        return container.AsReadOnly().FindFirstFit(item);
    }

    [Pure, MustUseReturnValue]
    public static (int x, int y) FindFirstFit(this in GridShape.ReadOnly container, in GridShape.ReadOnly item)
    {
        var maxY = container.Height - item.Height + 1;
        var maxX = container.Width - item.Width + 1;

        for (var y = 0; y < maxY; y++)
        for (var x = 0; x < maxX; x++)
            if (container.CanPlaceItem(item, (x, y)))
                return (x, y);

        return (-1, -1);
    }

    public static int PlaceMultipleShapes(
        this GridShape container,
        GridShape[] items,
        (int x, int y)[] outPositions)
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
    public static bool CanPlaceItem(this GridShape container, in GridShape.ReadOnly item, (int x, int y) pos)
    {
        return container.AsReadOnly().CanPlaceItem(item, pos);
    }

    [Pure, MustUseReturnValue]
    public static bool CanPlaceItem(this in GridShape.ReadOnly container, in GridShape.ReadOnly item, (int x, int y) pos)
    {
        // Bounds check
        if (pos.x < 0 || pos.y < 0 || pos.x + item.Width > container.Width || pos.y + item.Height > container.Height)
            return false;

        // Check each bit of the shape against the grid
        for (var sy = 0; sy < item.Height; sy++)
        for (var sx = 0; sx < item.Width; sx++)
        {
            var shapePos = (sx, sy);
            if (item.GetCell(shapePos))
            {
                var gridPos = (pos.x + sx, pos.y + sy);
                if (container.GetCell(gridPos))
                    return false;
            }
        }

        return true;
    }

    public static void PlaceItem(this GridShape container, in GridShape.ReadOnly item, (int x, int y) pos)
    {
        for (var sy = 0; sy < item.Height; sy++)
        for (var sx = 0; sx < item.Width; sx++)
        {
            var shapePos = (sx, sy);
            if (item.GetCell(shapePos))
            {
                var gridPos = (pos.x + sx, pos.y + sy);
                container.SetCell(gridPos, true);
            }
        }
    }

    public static void RemoveItem(this GridShape container, in GridShape.ReadOnly item, (int x, int y) pos)
    {
        for (var sy = 0; sy < item.Height; sy++)
        for (var sx = 0; sx < item.Width; sx++)
        {
            var shapePos = (sx, sy);
            if (item.GetCell(shapePos))
            {
                var gridPos = (pos.x + sx, pos.y + sy);
                container.SetCell(gridPos, false);
            }
        }
    }
}