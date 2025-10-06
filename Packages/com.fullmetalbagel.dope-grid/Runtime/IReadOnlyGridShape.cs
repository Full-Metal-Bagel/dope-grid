using System;
using JetBrains.Annotations;

namespace DopeGrid;

public interface IReadOnlyGridShape
{
    int Width { get; }
    int Height { get; }
    bool IsOccupied(int x, int y);
}

public interface IReadOnlyBitsGridShape : IReadOnlyGridShape<bool>
{
    ReadOnlySpanBitArray ReadOnlyBits { get; }
}

public interface IReadOnlyGridShape<out T> : IReadOnlyGridShape
{
    T this[int x, int y] { get; }
}

public static class ReadOnlyGridShapeExtensions
{
    [Pure, MustUseReturnValue]
    public static bool IsValuesEquals<TShape1, TShape2, TValue>(this TShape1 shape1, TShape2 shape2, TValue _ = default!)
        where TShape1 : IReadOnlyGridShape<TValue>
        where TShape2 : IReadOnlyGridShape<TValue>
        where TValue : IEquatable<TValue>
    {
        if (!HasSameSize(shape1, shape2))
            return false;

        for (var x = 0; x < shape1.Width; x++)
        for (var y = 0; y < shape2.Height; y++)
        {
            if (!shape1[x, y].Equals(shape2[x, y]))
                return false;
        }
        return true;
    }

    [Pure, MustUseReturnValue]
    public static int GetIndex<TShape>(this TShape shape, int x, int y)
        where TShape : IReadOnlyGridShape
        => y * shape.Width + x;

    [Pure, MustUseReturnValue]
    public static TValue GetCellValue<TShape, TValue>(this TShape shape, int x, int y, TValue _ = default!)
        where TShape : IReadOnlyGridShape<TValue>
        => shape[x, y];

    [Pure, MustUseReturnValue]
    public static int Size<TShape>(this TShape shape)
        where TShape : IReadOnlyGridShape
        => shape.Width * shape.Height;

    [Pure, MustUseReturnValue]
    public static bool HasSameSize<TShape1, TShape2>(this TShape1 shape1, TShape2 shape2)
        where TShape1 : IReadOnlyGridShape
        where TShape2 : IReadOnlyGridShape
        => shape1.Width == shape2.Width && shape1.Height == shape2.Height;

    [Pure, MustUseReturnValue]
    public static bool IsZeroSize<TShape>(this TShape shape)
        where TShape : IReadOnlyGridShape
        => shape.Width <= 0 || shape.Height <= 0;

    [Pure, MustUseReturnValue]
    public static int OccupiedSpaceCount<TShape>(this TShape shape)
        where TShape : IReadOnlyGridShape
    {
        var count = 0;
        for (var y = 0; y < shape.Height; y++)
        for (var x = 0; x < shape.Width; x++)
        {
            if (shape.IsOccupied(x, y)) count++;
        }
        return count;
    }

    [Pure, MustUseReturnValue]
    public static int FreeSpaceCount<TShape>(this TShape shape)
        where TShape : IReadOnlyGridShape
        => shape.Size() - shape.OccupiedSpaceCount();

    [Pure, MustUseReturnValue]
    public static bool Contains<TShape>(this TShape shape, int x, int y)
        where TShape : IReadOnlyGridShape
        => x >= 0 && x < shape.Width && y >= 0 && y < shape.Height;

    [Pure, MustUseReturnValue]
    public static int CountValue<TShape, TValue>(this TShape shape, TValue target)
        where TShape : IReadOnlyGridShape<TValue>
        where TValue : IEquatable<TValue>
        => Count<TShape, TValue, TValue>(shape, static (v, t) => v.Equals(t), target);

    [Pure, MustUseReturnValue]
    public static int CountWhere<TShape, TValue>(this TShape shape, Func<TValue, bool> predicate)
        where TShape : IReadOnlyGridShape<TValue>
        => Count<TShape, TValue, Func<TValue, bool>>(shape, static (value, p) => p(value), predicate);

    [Pure, MustUseReturnValue]
    public static int Count<TShape, TValue, TCaptureData>(this TShape shape, Func<TValue, TCaptureData, bool> predicate, TCaptureData data)
        where TShape : IReadOnlyGridShape<TValue>
    {
        if (predicate == null) throw new ArgumentNullException(nameof(predicate));

        int count = 0;
        for (int y = 0; y < shape.Height; y++)
        for (int x = 0; x < shape.Width; x++)
        {
            if (predicate(shape[x, y], data))
                count++;
        }

        return count;
    }

    [Pure, MustUseReturnValue]
    public static bool Any<TShape, TValue>(this TShape shape, Func<TValue, bool> predicate)
        where TShape : IReadOnlyGridShape<TValue>
        => Any<TShape, TValue, Func<TValue, bool>>(shape, static (v, p) => p(v), predicate);

    [Pure, MustUseReturnValue]
    public static bool Any<TShape, TValue, TCaptureData>(this TShape shape, Func<TValue, TCaptureData, bool> predicate, TCaptureData data)
        where TShape : IReadOnlyGridShape<TValue>
    {
        if (predicate == null) throw new ArgumentNullException(nameof(predicate));

        for (int y = 0; y < shape.Height; y++)
        for (int x = 0; x < shape.Width; x++)
            if (predicate(shape[x, y], data))
                return true;
        return false;
    }

    [Pure, MustUseReturnValue]
    public static bool All<TShape, TValue>(this TShape shape, Func<TValue, bool> predicate)
        where TShape : IReadOnlyGridShape<TValue>
        => All<TShape, TValue, Func<TValue, bool>>(shape, static (v, p) => p(v), predicate);

    [Pure, MustUseReturnValue]
    public static bool All<TShape, TValue, TCaptureData>(this TShape shape, Func<TValue, TCaptureData, bool> predicate, TCaptureData data)
        where TShape : IReadOnlyGridShape<TValue>
    {
        if (predicate == null) throw new ArgumentNullException(nameof(predicate));

        for (int y = 0; y < shape.Height; y++)
        for (int x = 0; x < shape.Width; x++)
            if (!predicate(shape[x, y], data))
                return false;
        return true;
    }

    [Pure, MustUseReturnValue]
    public static bool IsTrimmed<TShape, TValue>(this TShape shape, TValue _ = default!)
        where TShape : IReadOnlyGridShape<TValue>
    {
        var (x, y, width, height) = GetTrimmedBound(shape, _);
        return x == 0 && y == 0 && width == shape.Width && height == shape.Height;
    }

    [Pure, MustUseReturnValue]
    public static (int offsetX, int offsetY, int width, int height) GetTrimmedBound<TShape, TValue>(this TShape shape, TValue _ = default!)
        where TShape : IReadOnlyGridShape<TValue>
    {
        int minX = shape.Width, minY = shape.Height;
        int maxX = -1, maxY = -1;

        for (int y = 0; y < shape.Height; y++)
        for (int x = 0; x < shape.Width; x++)
        {
            if (shape.IsOccupied(x, y))
            {
                if (x < minX) minX = x;
                if (x > maxX) maxX = x;
                if (y < minY) minY = y;
                if (y > maxY) maxY = y;
            }
        }

        if (maxX < 0 || maxY < 0) return (0, 0, 0, 0);

        var width = maxX - minX + 1;
        var height = maxY - minY + 1;
        return (minX, minY, width, height);
    }

    [Pure, MustUseReturnValue]
    public static bool CanPlaceItem<TGrid, TShape, TValue>(this TGrid grid, TShape shape, int x, int y, TValue _ = default!)
        where TGrid : IReadOnlyGridShape<TValue>
        where TShape : IReadOnlyGridShape<bool>
    {
        if (x < 0 || y < 0 || x + shape.Width > grid.Width || y + shape.Height > grid.Height)
            return false;

        for (var sy = 0; sy < shape.Height; sy++)
        for (var sx = 0; sx < shape.Width; sx++)
        {
            if (shape.IsOccupied(sx, sy))
            {
                var gx = x + sx;
                var gy = y + sy;
                if (grid.IsOccupied(gx, gy))
                    return false;
            }
        }
        return true;
    }

    [Pure, MustUseReturnValue]
    public static (int x, int y, RotationDegree rotation) FindFirstFitWithFreeRotation<TGrid, TValue>(this TGrid grid, ImmutableGridShape shape, TValue _ = default!)
        where TGrid : IReadOnlyGridShape<TValue>
    {
        var rotateCount = 0;
        var rotatedItem = shape;
        (int x, int y) position;
        do
        {
            position = FindFirstFitWithFixedRotation(grid, rotatedItem, _);
            if (position.x >= 0) break;
            rotatedItem = rotatedItem.Rotate90();
            rotateCount++;
        } while (rotatedItem != shape);

        var rotation = rotateCount switch
        {
            0 => RotationDegree.None,
            1 => RotationDegree.Clockwise90,
            2 => RotationDegree.Clockwise180,
            3 => RotationDegree.Clockwise270,
            _ => throw new NotImplementedException()
        };

        return (position.x, position.y, rotation);
    }

    [Pure, MustUseReturnValue]
    public static bool CheckShapeCells<TGrid, TShape, TValue>(this TGrid grid, TShape shape, int x, int y, Func<int, int, TValue, bool> cellPredicate)
        where TGrid : IReadOnlyGridShape<TValue>
        where TShape : IReadOnlyGridShape<bool>
        => CheckShapeCells<TGrid, TShape, TValue, Func<int, int, TValue, bool>>(grid, shape, x, y, static (gx, gy, value, pred) => pred(gx, gy, value), cellPredicate);

    [Pure, MustUseReturnValue]
    public static bool CheckShapeCells<TGrid, TShape, TValue, TData>(this TGrid grid, TShape shape, int x, int y, Func<int, int, TValue, TData, bool> cellPredicate, TData data)
        where TGrid : IReadOnlyGridShape<TValue>
        where TShape : IReadOnlyGridShape<bool>
    {
        if (cellPredicate == null) throw new ArgumentNullException(nameof(cellPredicate));

        for (int sy = 0; sy < shape.Height; sy++)
        for (int sx = 0; sx < shape.Width; sx++)
        {
            if (shape.IsOccupied(sx, sy))
            {
                var gx = x + sx;
                var gy = y + sy;
                if (!Contains(grid, gx, gy))
                    return false;
                if (!cellPredicate(gx, gy, grid[gx, gy], data))
                    return false;
            }
        }
        return true;
    }

    [Pure, MustUseReturnValue]
    public static (int x, int y) FindFirstFitWithFixedRotation<TGrid, TShape, TValue>(this TGrid grid, TShape item, TValue _ = default!)
        where TGrid : IReadOnlyGridShape<TValue>
        where TShape : IReadOnlyGridShape<bool>
    {
        var maxY = grid.Height - item.Height + 1;
        var maxX = grid.Width - item.Width + 1;
        for (var y = 0; y < maxY; y++)
        for (var x = 0; x < maxX; x++)
            if (grid.CanPlaceItem(item, x, y, _))
                return (x, y);
        return (-1, -1);
    }

    [Pure, MustUseReturnValue]
    public static bool IsWithinBounds<TGrid, TShape, TValue>(this TGrid grid, TShape shape, int x, int y, TValue _ = default!)
        where TGrid : IReadOnlyGridShape<TValue>
        where TShape : IReadOnlyGridShape<bool>
    {
        return x >= 0 && y >= 0 && x + shape.Width <= grid.Width && y + shape.Height <= grid.Height;
    }
}

public static class ReadOnlyBoolGridShapeExtensions
{
    [Pure, MustUseReturnValue]
    public static bool IsValuesEquals<TGrid>(this TGrid grid1, TGrid grid2)
        where TGrid : IReadOnlyGridShape<bool>
        => grid1.IsValuesEquals<TGrid, TGrid, bool>(grid2);

    [Pure, MustUseReturnValue]
    public static bool GetCellValue<T>(this T shape, int x, int y)
        where T : IReadOnlyGridShape<bool>
        => shape.GetCellValue<T, bool>(x, y);

    [Pure, MustUseReturnValue]
    public static int CountValue<T>(this T shape, bool target)
        where T : IReadOnlyGridShape<bool>
        => shape.CountValue<T, bool>(target);

    [Pure, MustUseReturnValue]
    public static int CountWhere<T>(this T shape, Func<bool, bool> predicate)
        where T : IReadOnlyGridShape<bool>
        => shape.CountWhere<T, bool>(predicate);

    [Pure, MustUseReturnValue]
    public static int Count<T, TCaptureData>(this T shape,
        Func<bool, TCaptureData, bool> predicate,
        TCaptureData data)
        where T : IReadOnlyGridShape<bool>
        => shape.Count<T, bool, TCaptureData>(predicate, data);

    [Pure, MustUseReturnValue]
    public static bool Any<T>(this T shape, Func<bool, bool> predicate)
        where T : IReadOnlyGridShape<bool>
        => shape.Any<T, bool>(predicate);

    [Pure, MustUseReturnValue]
    public static bool Any<T, TCaptureData>(this T shape,
        Func<bool, TCaptureData, bool> predicate,
        TCaptureData data)
        where T : IReadOnlyGridShape<bool>
        => shape.Any<T, bool, TCaptureData>(predicate, data);

    [Pure, MustUseReturnValue]
    public static bool All<T>(this T shape, Func<bool, bool> predicate)
        where T : IReadOnlyGridShape<bool>
        => shape.All<T, bool>(predicate);

    [Pure, MustUseReturnValue]
    public static bool All<T, TCaptureData>(this T shape,
        Func<bool, TCaptureData, bool> predicate,
        TCaptureData data)
        where T : IReadOnlyGridShape<bool>
        => shape.All<T, bool, TCaptureData>(predicate, data);

    [Pure, MustUseReturnValue]
    public static bool IsTrimmed<T>(this T shape)
        where T : IReadOnlyGridShape<bool>
        => shape.IsTrimmed<T, bool>();

    [Pure, MustUseReturnValue]
    public static bool CanPlaceItem<TGrid, TShape>(this TGrid grid, TShape shape, int x, int y)
        where TGrid : IReadOnlyGridShape<bool>
        where TShape : IReadOnlyGridShape<bool>
        => grid.CanPlaceItem<TGrid, TShape, bool>(shape, x, y);

    [Pure, MustUseReturnValue]
    public static (int x, int y, RotationDegree rotation) FindFirstFitWithFreeRotation<TGrid>( this TGrid grid, ImmutableGridShape item)
        where TGrid : IReadOnlyGridShape<bool>
    => grid.FindFirstFitWithFreeRotation<TGrid, bool>(item);

    [Pure, MustUseReturnValue]
    public static bool CheckShapeCells<TGrid, TShape>(this TGrid grid,
        TShape shape,
        int x, int y,
        Func<int, int, bool, bool> cellPredicate)
        where TGrid : IReadOnlyGridShape<bool>
        where TShape : IReadOnlyGridShape<bool>
    => grid.CheckShapeCells(shape, x, y, static (gx, gy, value, pred) => pred(gx, gy, value), cellPredicate);

    [Pure, MustUseReturnValue]
    public static bool CheckShapeCells<TGrid, TShape, TData>(this TGrid grid,
        TShape shape,
        int x, int y,
        Func<int, int, bool, TData, bool> cellPredicate,
        TData data)
        where TGrid : IReadOnlyGridShape<bool>
        where TShape : IReadOnlyGridShape<bool>
    => grid.CheckShapeCells<TGrid, TShape, bool, TData>(shape, x, y, cellPredicate, data);

    [Pure, MustUseReturnValue]
    public static (int x, int y) FindFirstFitWithFixedRotation<TGrid, TShape>(this TGrid grid, TShape shape)
        where TGrid : IReadOnlyGridShape<bool>
        where TShape : IReadOnlyGridShape<bool>
    => grid.FindFirstFitWithFixedRotation<TGrid, TShape, bool>(shape);

    [Pure, MustUseReturnValue]
    public static bool IsWithinBounds<T, TMask>(this T grid, TMask shape, int x, int y)
        where T : IReadOnlyGridShape<bool>
        where TMask : IReadOnlyGridShape<bool>
    => grid.IsWithinBounds<T, TMask, bool>(shape, x, y);

}
