using System;
using JetBrains.Annotations;
using Unity.Collections;

namespace DopeGrid.Native;

public static class GridBoardExtension
{
    public static int PlaceMultipleShapes(
        this GridShape container,
        NativeArray<ImmutableGridShape> items,
        NativeArray<GridPosition> outPositions)
    {
        var placed = 0;

        var readonlyContainer = container.AsReadOnly();
        for (var i = 0; i < items.Length; i++)
        {
            var pos = readonlyContainer.FindFirstFitWithFixedRotation(items[i]);
            if (pos.X >= 0)
            {
                container.PlaceItem(items[i], pos);
                outPositions[placed] = pos;
                placed++;
            }
        }

        return placed;
    }

    public static void PlaceItem(this GridShape container, ImmutableGridShape item, GridPosition pos)
    {
        for (var sy = 0; sy < item.Height; sy++)
        for (var sx = 0; sx < item.Width; sx++)
        {
            var shapePos = new GridPosition(sx, sy);
            if (item.GetCellValue(shapePos))
            {
                var gridPos = new GridPosition(pos.X + sx, pos.Y + sy);
                container.SetCell(gridPos, true);
            }
        }
    }

    public static void RemoveItem(this GridShape container, ImmutableGridShape item, GridPosition pos)
    {
        for (var sy = 0; sy < item.Height; sy++)
        for (var sx = 0; sx < item.Width; sx++)
        {
            var shapePos = new GridPosition(sx, sy);
            if (item.GetCellValue(shapePos))
            {
                var gridPos = new GridPosition(pos.X + sx, pos.Y + sy);
                container.SetCell(gridPos, false);
            }
        }
    }
}
