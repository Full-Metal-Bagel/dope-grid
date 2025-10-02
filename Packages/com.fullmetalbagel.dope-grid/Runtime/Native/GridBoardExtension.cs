using System;
using JetBrains.Annotations;
using Unity.Collections;

namespace DopeGrid.Native;

public static class GridBoardExtension
{
    [Pure, MustUseReturnValue]
    public static GridPosition FindFirstFitWithFixedRotation(this GridShape container, ImmutableGridShape item)
    {
        return container.AsReadOnly().FindFirstFitWithFixedRotation(item);
    }

    [Pure, MustUseReturnValue]
    public static GridPosition FindFirstFitWithFixedRotation(this in GridShape.ReadOnly container, ImmutableGridShape item)
    {
        var maxY = container.Height - item.Height + 1;
        var maxX = container.Width - item.Width + 1;

        for (var y = 0; y < maxY; y++)
        for (var x = 0; x < maxX; x++)
            if (container.CanPlaceItem(item, new GridPosition(x, y)))
                return new GridPosition(x, y);

        return GridPosition.Invalid;
    }

    [Pure, MustUseReturnValue]
    public static (GridPosition position, RotationDegree rotation) FindFirstFitWithFreeRotation(this in GridShape container, ImmutableGridShape item)
    {
        return container.AsReadOnly().FindFirstFitWithFreeRotation(item);
    }

    [Pure, MustUseReturnValue]
    public static (GridPosition position, RotationDegree rotation) FindFirstFitWithFreeRotation(this in GridShape.ReadOnly container, ImmutableGridShape item)
    {
        var rotateCount = 0;
        var rotatedItem = item;
        var position = GridPosition.Invalid;
        do
        {
            position = FindFirstFitWithFixedRotation(container, rotatedItem);
            if (position.IsValid) break;
            rotatedItem = rotatedItem.Rotate90();
            rotateCount++;
        }
        while (rotatedItem != item);

        var rotation = rotateCount switch
        {
            0 => RotationDegree.None,
            1 => RotationDegree.Clockwise90,
            2 => RotationDegree.Clockwise180,
            3 => RotationDegree.Clockwise270,
            _ => throw new NotImplementedException()
        };
        return (position, rotation);
    }

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

    [Pure, MustUseReturnValue]
    public static bool CanPlaceItem(this GridShape container, ImmutableGridShape item, GridPosition pos)
    {
        return container.AsReadOnly().CanPlaceItem(item, pos);
    }

    [Pure, MustUseReturnValue]
    public static bool CanPlaceItem(this in GridShape.ReadOnly container, ImmutableGridShape item, GridPosition pos)
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

    public static void PlaceItem(this GridShape container, ImmutableGridShape item, GridPosition pos)
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

    public static void RemoveItem(this GridShape container, ImmutableGridShape item, GridPosition pos)
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
