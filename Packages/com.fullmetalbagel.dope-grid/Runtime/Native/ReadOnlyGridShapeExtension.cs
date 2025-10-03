

namespace DopeGrid.Native;

public static partial class ReadOnlyGridShapeExtension
{
    
#region GridShape

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static int GetIndex(this in GridShape shape, GridPosition pos) => GetIndex(shape, pos.X, pos.Y);

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static int GetIndex(this in GridShape shape, int x, int y) => y * shape.Width + x;

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool GetCellValue(this in GridShape shape, GridPosition pos) => shape[pos];

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool GetCellValue(this in GridShape shape, int x, int y) => GetCellValue(shape, new GridPosition(x, y));

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static int Size(this in GridShape shape) => shape.Width * shape.Height;

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool IsEmpty(this in GridShape shape) => shape.Width <= 0 || shape.Height <= 0;

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool Contains(this in GridShape shape, GridPosition pos) => Contains(shape, pos.X, pos.Y);

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool Contains(this in GridShape shape, int x, int y) => x >= 0 && x < shape.Width && y >= 0 && y < shape.Height;

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static int CountValue(this in GridShape shape, bool target)
    {
        return Count(shape, (v, t) => System.Collections.Generic.EqualityComparer<bool>.Default.Equals(v, t), target);
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static int CountWhere(this in GridShape shape, System.Func<bool, bool> predicate)
    {
        return Count(shape, (value, p) => p(value), predicate);
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static int Count<TCaptureData>(this in GridShape shape, System.Func<bool, TCaptureData, bool> predicate, TCaptureData data)
    {
        int count = 0;
        var size = shape.Width * shape.Height;
        for (int i = 0; i < size; i++)
        {
            if (predicate(shape[i], data))
                count++;
        }
        return count;
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool Any(this in GridShape shape, System.Func<bool, bool> predicate)
    {
        return Any(shape, (v, p) => p(v), predicate);
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool Any<TCaptureData>(this in GridShape shape, System.Func<bool, TCaptureData, bool> predicate, TCaptureData data)
    {
        var size = shape.Width * shape.Height;
        for (int i = 0; i < size; i++)
        {
            if (predicate(shape[i], data))
                return true;
        }
        return false;
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool All(this in GridShape shape, System.Func<bool, bool> predicate)
    {
        return All(shape, (v, p) => p(v), predicate);
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool All<TCaptureData>(this in GridShape shape, System.Func<bool, TCaptureData, bool> predicate, TCaptureData data)
    {
        var size = shape.Width * shape.Height;
        for (int i = 0; i < size; i++)
        {
            if (!predicate(shape[i], data))
                return false;
        }
        return true;
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool IsTrimmed(this in GridShape shape, bool freeValue = default)
    {
        // Empty shapes are considered trimmed
        if (shape.Width == 0 || shape.Height == 0)
            return true;

        // A shape is trimmed when all four borders have at least one occupied cell
        return HasOccupiedCellInRow(shape, 0, freeValue) &&                    // Top row
               HasOccupiedCellInRow(shape, shape.Height - 1, freeValue) &&     // Bottom row
               HasOccupiedCellInColumn(shape, 0, freeValue) &&                 // Left column
               HasOccupiedCellInColumn(shape, shape.Width - 1, freeValue);     // Right column

        static bool HasOccupiedCellInRow(in GridShape shape, int row, bool freeValue)
        {
            for (var column = 0; column < shape.Width; column++)
            {
                var value = shape.GetCellValue(column, row);
                var isFree = value.Equals(freeValue);
                if (!isFree) return true;
            }
            return false;
        }

        static bool HasOccupiedCellInColumn(in GridShape shape, int column, bool freeValue)
        {
            for (var row = 0; row < shape.Height; row++)
            {
                var value = shape.GetCellValue(column, row);
                var isFree = value.Equals(freeValue);
                if (!isFree) return true;
            }
            return false;
        }
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool CanPlaceItem(this in GridShape container, ImmutableGridShape item, GridPosition pos, bool freeValue = default)
    {
        // Bounds check
        if (pos.X < 0 || pos.Y < 0 || pos.X + item.Width > container.Width || pos.Y + item.Height > container.Height)
            return false;

        // Check each bit of the shape against the grid
        for (var sy = 0; sy < item.Height; sy++)
        for (var sx = 0; sx < item.Width; sx++)
        {
            var shapePos = new GridPosition(sx, sy);
            if (item.GetCellValue(shapePos))
            {
                var gridPos = new GridPosition(pos.X + sx, pos.Y + sy);
                if (container.GetCellValue(gridPos).Equals(freeValue))
                    return false;
            }
        }

        return true;
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static (GridPosition position, RotationDegree rotation) FindFirstFitWithFreeRotation(this in GridShape container, ImmutableGridShape item)
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
            _ => throw new System.NotImplementedException()
        };

        return (position, rotation);
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool CheckShapeCells<TData>(this in GridShape grid, ImmutableGridShape shape, GridPosition position, System.Func<GridPosition, bool, TData, bool> cellPredicate, TData data)
    {
        for (int y = 0; y < shape.Height; y++)
        {
            for (int x = 0; x < shape.Width; x++)
            {
                if (shape[x, y]) // Shape cell is occupied
                {
                    var gridPos = new GridPosition(position.X + x, position.Y + y);

                    // Check bounds
                    if (!grid.Contains(gridPos))
                        return false;

                    // Check predicate
                    if (!cellPredicate(gridPos, grid[gridPos], data))
                        return false;
                }
            }
        }

        return true;
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static GridPosition FindFirstFitWithFixedRotation(this GridShape grid, ImmutableGridShape item, bool freeValue = default)
    {
        var maxY = grid.Height - item.Height + 1;
        var maxX = grid.Width - item.Width + 1;

        for (var y = 0; y < maxY; y++)
        for (var x = 0; x < maxX; x++)
            if (CanPlaceItem(grid, item, new GridPosition(x, y), freeValue))
                return new GridPosition(x, y);

        return GridPosition.Invalid;
    }

#endregion
    
#region GridShape.ReadOnly

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static int GetIndex(this in GridShape.ReadOnly shape, GridPosition pos) => GetIndex(shape, pos.X, pos.Y);

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static int GetIndex(this in GridShape.ReadOnly shape, int x, int y) => y * shape.Width + x;

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool GetCellValue(this in GridShape.ReadOnly shape, GridPosition pos) => shape[pos];

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool GetCellValue(this in GridShape.ReadOnly shape, int x, int y) => GetCellValue(shape, new GridPosition(x, y));

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static int Size(this in GridShape.ReadOnly shape) => shape.Width * shape.Height;

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool IsEmpty(this in GridShape.ReadOnly shape) => shape.Width <= 0 || shape.Height <= 0;

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool Contains(this in GridShape.ReadOnly shape, GridPosition pos) => Contains(shape, pos.X, pos.Y);

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool Contains(this in GridShape.ReadOnly shape, int x, int y) => x >= 0 && x < shape.Width && y >= 0 && y < shape.Height;

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static int CountValue(this in GridShape.ReadOnly shape, bool target)
    {
        return Count(shape, (v, t) => System.Collections.Generic.EqualityComparer<bool>.Default.Equals(v, t), target);
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static int CountWhere(this in GridShape.ReadOnly shape, System.Func<bool, bool> predicate)
    {
        return Count(shape, (value, p) => p(value), predicate);
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static int Count<TCaptureData>(this in GridShape.ReadOnly shape, System.Func<bool, TCaptureData, bool> predicate, TCaptureData data)
    {
        int count = 0;
        var size = shape.Width * shape.Height;
        for (int i = 0; i < size; i++)
        {
            if (predicate(shape[i], data))
                count++;
        }
        return count;
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool Any(this in GridShape.ReadOnly shape, System.Func<bool, bool> predicate)
    {
        return Any(shape, (v, p) => p(v), predicate);
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool Any<TCaptureData>(this in GridShape.ReadOnly shape, System.Func<bool, TCaptureData, bool> predicate, TCaptureData data)
    {
        var size = shape.Width * shape.Height;
        for (int i = 0; i < size; i++)
        {
            if (predicate(shape[i], data))
                return true;
        }
        return false;
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool All(this in GridShape.ReadOnly shape, System.Func<bool, bool> predicate)
    {
        return All(shape, (v, p) => p(v), predicate);
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool All<TCaptureData>(this in GridShape.ReadOnly shape, System.Func<bool, TCaptureData, bool> predicate, TCaptureData data)
    {
        var size = shape.Width * shape.Height;
        for (int i = 0; i < size; i++)
        {
            if (!predicate(shape[i], data))
                return false;
        }
        return true;
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool IsTrimmed(this in GridShape.ReadOnly shape, bool freeValue = default)
    {
        // Empty shapes are considered trimmed
        if (shape.Width == 0 || shape.Height == 0)
            return true;

        // A shape is trimmed when all four borders have at least one occupied cell
        return HasOccupiedCellInRow(shape, 0, freeValue) &&                    // Top row
               HasOccupiedCellInRow(shape, shape.Height - 1, freeValue) &&     // Bottom row
               HasOccupiedCellInColumn(shape, 0, freeValue) &&                 // Left column
               HasOccupiedCellInColumn(shape, shape.Width - 1, freeValue);     // Right column

        static bool HasOccupiedCellInRow(in GridShape.ReadOnly shape, int row, bool freeValue)
        {
            for (var column = 0; column < shape.Width; column++)
            {
                var value = shape.GetCellValue(column, row);
                var isFree = value.Equals(freeValue);
                if (!isFree) return true;
            }
            return false;
        }

        static bool HasOccupiedCellInColumn(in GridShape.ReadOnly shape, int column, bool freeValue)
        {
            for (var row = 0; row < shape.Height; row++)
            {
                var value = shape.GetCellValue(column, row);
                var isFree = value.Equals(freeValue);
                if (!isFree) return true;
            }
            return false;
        }
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool CanPlaceItem(this in GridShape.ReadOnly container, ImmutableGridShape item, GridPosition pos, bool freeValue = default)
    {
        // Bounds check
        if (pos.X < 0 || pos.Y < 0 || pos.X + item.Width > container.Width || pos.Y + item.Height > container.Height)
            return false;

        // Check each bit of the shape against the grid
        for (var sy = 0; sy < item.Height; sy++)
        for (var sx = 0; sx < item.Width; sx++)
        {
            var shapePos = new GridPosition(sx, sy);
            if (item.GetCellValue(shapePos))
            {
                var gridPos = new GridPosition(pos.X + sx, pos.Y + sy);
                if (container.GetCellValue(gridPos).Equals(freeValue))
                    return false;
            }
        }

        return true;
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
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
            _ => throw new System.NotImplementedException()
        };

        return (position, rotation);
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool CheckShapeCells<TData>(this in GridShape.ReadOnly grid, ImmutableGridShape shape, GridPosition position, System.Func<GridPosition, bool, TData, bool> cellPredicate, TData data)
    {
        for (int y = 0; y < shape.Height; y++)
        {
            for (int x = 0; x < shape.Width; x++)
            {
                if (shape[x, y]) // Shape cell is occupied
                {
                    var gridPos = new GridPosition(position.X + x, position.Y + y);

                    // Check bounds
                    if (!grid.Contains(gridPos))
                        return false;

                    // Check predicate
                    if (!cellPredicate(gridPos, grid[gridPos], data))
                        return false;
                }
            }
        }

        return true;
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static GridPosition FindFirstFitWithFixedRotation(this GridShape.ReadOnly grid, ImmutableGridShape item, bool freeValue = default)
    {
        var maxY = grid.Height - item.Height + 1;
        var maxX = grid.Width - item.Width + 1;

        for (var y = 0; y < maxY; y++)
        for (var x = 0; x < maxX; x++)
            if (CanPlaceItem(grid, item, new GridPosition(x, y), freeValue))
                return new GridPosition(x, y);

        return GridPosition.Invalid;
    }

#endregion
    
#region ValueGridShape<T>

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static int GetIndex<T>(this in ValueGridShape<T> shape, GridPosition pos) where T : unmanaged, System.IEquatable<T> => GetIndex(shape, pos.X, pos.Y);

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static int GetIndex<T>(this in ValueGridShape<T> shape, int x, int y) where T : unmanaged, System.IEquatable<T> => y * shape.Width + x;

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static T GetCellValue<T>(this in ValueGridShape<T> shape, GridPosition pos) where T : unmanaged, System.IEquatable<T> => shape[pos];

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static T GetCellValue<T>(this in ValueGridShape<T> shape, int x, int y) where T : unmanaged, System.IEquatable<T> => GetCellValue(shape, new GridPosition(x, y));

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static int Size<T>(this in ValueGridShape<T> shape) where T : unmanaged, System.IEquatable<T> => shape.Width * shape.Height;

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool IsEmpty<T>(this in ValueGridShape<T> shape) where T : unmanaged, System.IEquatable<T> => shape.Width <= 0 || shape.Height <= 0;

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool Contains<T>(this in ValueGridShape<T> shape, GridPosition pos) where T : unmanaged, System.IEquatable<T> => Contains(shape, pos.X, pos.Y);

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool Contains<T>(this in ValueGridShape<T> shape, int x, int y) where T : unmanaged, System.IEquatable<T> => x >= 0 && x < shape.Width && y >= 0 && y < shape.Height;

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static int CountValue<T>(this in ValueGridShape<T> shape, T target) where T : unmanaged, System.IEquatable<T>
    {
        return Count(shape, (v, t) => System.Collections.Generic.EqualityComparer<T>.Default.Equals(v, t), target);
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static int CountWhere<T>(this in ValueGridShape<T> shape, System.Func<T, bool> predicate) where T : unmanaged, System.IEquatable<T>
    {
        return Count(shape, (value, p) => p(value), predicate);
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static int Count<T, TCaptureData>(this in ValueGridShape<T> shape, System.Func<T, TCaptureData, bool> predicate, TCaptureData data) where T : unmanaged, System.IEquatable<T>
    {
        int count = 0;
        var size = shape.Width * shape.Height;
        for (int i = 0; i < size; i++)
        {
            if (predicate(shape[i], data))
                count++;
        }
        return count;
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool Any<T>(this in ValueGridShape<T> shape, System.Func<T, bool> predicate) where T : unmanaged, System.IEquatable<T>
    {
        return Any(shape, (v, p) => p(v), predicate);
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool Any<T, TCaptureData>(this in ValueGridShape<T> shape, System.Func<T, TCaptureData, bool> predicate, TCaptureData data) where T : unmanaged, System.IEquatable<T>
    {
        var size = shape.Width * shape.Height;
        for (int i = 0; i < size; i++)
        {
            if (predicate(shape[i], data))
                return true;
        }
        return false;
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool All<T>(this in ValueGridShape<T> shape, System.Func<T, bool> predicate) where T : unmanaged, System.IEquatable<T>
    {
        return All(shape, (v, p) => p(v), predicate);
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool All<T, TCaptureData>(this in ValueGridShape<T> shape, System.Func<T, TCaptureData, bool> predicate, TCaptureData data) where T : unmanaged, System.IEquatable<T>
    {
        var size = shape.Width * shape.Height;
        for (int i = 0; i < size; i++)
        {
            if (!predicate(shape[i], data))
                return false;
        }
        return true;
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool IsTrimmed<T>(this in ValueGridShape<T> shape, T freeValue = default) where T : unmanaged, System.IEquatable<T>
    {
        // Empty shapes are considered trimmed
        if (shape.Width == 0 || shape.Height == 0)
            return true;

        // A shape is trimmed when all four borders have at least one occupied cell
        return HasOccupiedCellInRow(shape, 0, freeValue) &&                    // Top row
               HasOccupiedCellInRow(shape, shape.Height - 1, freeValue) &&     // Bottom row
               HasOccupiedCellInColumn(shape, 0, freeValue) &&                 // Left column
               HasOccupiedCellInColumn(shape, shape.Width - 1, freeValue);     // Right column

        static bool HasOccupiedCellInRow(in ValueGridShape<T> shape, int row, T freeValue)
        {
            for (var column = 0; column < shape.Width; column++)
            {
                var value = shape.GetCellValue(column, row);
                var isFree = value.Equals(freeValue);
                if (!isFree) return true;
            }
            return false;
        }

        static bool HasOccupiedCellInColumn(in ValueGridShape<T> shape, int column, T freeValue)
        {
            for (var row = 0; row < shape.Height; row++)
            {
                var value = shape.GetCellValue(column, row);
                var isFree = value.Equals(freeValue);
                if (!isFree) return true;
            }
            return false;
        }
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool CanPlaceItem<T>(this in ValueGridShape<T> container, ImmutableGridShape item, GridPosition pos, T freeValue = default) where T : unmanaged, System.IEquatable<T>
    {
        // Bounds check
        if (pos.X < 0 || pos.Y < 0 || pos.X + item.Width > container.Width || pos.Y + item.Height > container.Height)
            return false;

        // Check each bit of the shape against the grid
        for (var sy = 0; sy < item.Height; sy++)
        for (var sx = 0; sx < item.Width; sx++)
        {
            var shapePos = new GridPosition(sx, sy);
            if (item.GetCellValue(shapePos))
            {
                var gridPos = new GridPosition(pos.X + sx, pos.Y + sy);
                if (container.GetCellValue(gridPos).Equals(freeValue))
                    return false;
            }
        }

        return true;
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static (GridPosition position, RotationDegree rotation) FindFirstFitWithFreeRotation<T>(this in ValueGridShape<T> container, ImmutableGridShape item) where T : unmanaged, System.IEquatable<T>
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
            _ => throw new System.NotImplementedException()
        };

        return (position, rotation);
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool CheckShapeCells<T, TData>(this in ValueGridShape<T> grid, ImmutableGridShape shape, GridPosition position, System.Func<GridPosition, T, TData, bool> cellPredicate, TData data) where T : unmanaged, System.IEquatable<T>
    {
        for (int y = 0; y < shape.Height; y++)
        {
            for (int x = 0; x < shape.Width; x++)
            {
                if (shape[x, y]) // Shape cell is occupied
                {
                    var gridPos = new GridPosition(position.X + x, position.Y + y);

                    // Check bounds
                    if (!grid.Contains(gridPos))
                        return false;

                    // Check predicate
                    if (!cellPredicate(gridPos, grid[gridPos], data))
                        return false;
                }
            }
        }

        return true;
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static GridPosition FindFirstFitWithFixedRotation<T>(this ValueGridShape<T> grid, ImmutableGridShape item, T freeValue = default) where T : unmanaged, System.IEquatable<T>
    {
        var maxY = grid.Height - item.Height + 1;
        var maxX = grid.Width - item.Width + 1;

        for (var y = 0; y < maxY; y++)
        for (var x = 0; x < maxX; x++)
            if (CanPlaceItem(grid, item, new GridPosition(x, y), freeValue))
                return new GridPosition(x, y);

        return GridPosition.Invalid;
    }

#endregion
    
#region ValueGridShape<T>.ReadOnly

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static int GetIndex<T>(this in ValueGridShape<T>.ReadOnly shape, GridPosition pos) where T : unmanaged, System.IEquatable<T> => GetIndex(shape, pos.X, pos.Y);

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static int GetIndex<T>(this in ValueGridShape<T>.ReadOnly shape, int x, int y) where T : unmanaged, System.IEquatable<T> => y * shape.Width + x;

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static T GetCellValue<T>(this in ValueGridShape<T>.ReadOnly shape, GridPosition pos) where T : unmanaged, System.IEquatable<T> => shape[pos];

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static T GetCellValue<T>(this in ValueGridShape<T>.ReadOnly shape, int x, int y) where T : unmanaged, System.IEquatable<T> => GetCellValue(shape, new GridPosition(x, y));

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static int Size<T>(this in ValueGridShape<T>.ReadOnly shape) where T : unmanaged, System.IEquatable<T> => shape.Width * shape.Height;

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool IsEmpty<T>(this in ValueGridShape<T>.ReadOnly shape) where T : unmanaged, System.IEquatable<T> => shape.Width <= 0 || shape.Height <= 0;

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool Contains<T>(this in ValueGridShape<T>.ReadOnly shape, GridPosition pos) where T : unmanaged, System.IEquatable<T> => Contains(shape, pos.X, pos.Y);

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool Contains<T>(this in ValueGridShape<T>.ReadOnly shape, int x, int y) where T : unmanaged, System.IEquatable<T> => x >= 0 && x < shape.Width && y >= 0 && y < shape.Height;

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static int CountValue<T>(this in ValueGridShape<T>.ReadOnly shape, T target) where T : unmanaged, System.IEquatable<T>
    {
        return Count(shape, (v, t) => System.Collections.Generic.EqualityComparer<T>.Default.Equals(v, t), target);
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static int CountWhere<T>(this in ValueGridShape<T>.ReadOnly shape, System.Func<T, bool> predicate) where T : unmanaged, System.IEquatable<T>
    {
        return Count(shape, (value, p) => p(value), predicate);
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static int Count<T, TCaptureData>(this in ValueGridShape<T>.ReadOnly shape, System.Func<T, TCaptureData, bool> predicate, TCaptureData data) where T : unmanaged, System.IEquatable<T>
    {
        int count = 0;
        var size = shape.Width * shape.Height;
        for (int i = 0; i < size; i++)
        {
            if (predicate(shape[i], data))
                count++;
        }
        return count;
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool Any<T>(this in ValueGridShape<T>.ReadOnly shape, System.Func<T, bool> predicate) where T : unmanaged, System.IEquatable<T>
    {
        return Any(shape, (v, p) => p(v), predicate);
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool Any<T, TCaptureData>(this in ValueGridShape<T>.ReadOnly shape, System.Func<T, TCaptureData, bool> predicate, TCaptureData data) where T : unmanaged, System.IEquatable<T>
    {
        var size = shape.Width * shape.Height;
        for (int i = 0; i < size; i++)
        {
            if (predicate(shape[i], data))
                return true;
        }
        return false;
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool All<T>(this in ValueGridShape<T>.ReadOnly shape, System.Func<T, bool> predicate) where T : unmanaged, System.IEquatable<T>
    {
        return All(shape, (v, p) => p(v), predicate);
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool All<T, TCaptureData>(this in ValueGridShape<T>.ReadOnly shape, System.Func<T, TCaptureData, bool> predicate, TCaptureData data) where T : unmanaged, System.IEquatable<T>
    {
        var size = shape.Width * shape.Height;
        for (int i = 0; i < size; i++)
        {
            if (!predicate(shape[i], data))
                return false;
        }
        return true;
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool IsTrimmed<T>(this in ValueGridShape<T>.ReadOnly shape, T freeValue = default) where T : unmanaged, System.IEquatable<T>
    {
        // Empty shapes are considered trimmed
        if (shape.Width == 0 || shape.Height == 0)
            return true;

        // A shape is trimmed when all four borders have at least one occupied cell
        return HasOccupiedCellInRow(shape, 0, freeValue) &&                    // Top row
               HasOccupiedCellInRow(shape, shape.Height - 1, freeValue) &&     // Bottom row
               HasOccupiedCellInColumn(shape, 0, freeValue) &&                 // Left column
               HasOccupiedCellInColumn(shape, shape.Width - 1, freeValue);     // Right column

        static bool HasOccupiedCellInRow(in ValueGridShape<T>.ReadOnly shape, int row, T freeValue)
        {
            for (var column = 0; column < shape.Width; column++)
            {
                var value = shape.GetCellValue(column, row);
                var isFree = value.Equals(freeValue);
                if (!isFree) return true;
            }
            return false;
        }

        static bool HasOccupiedCellInColumn(in ValueGridShape<T>.ReadOnly shape, int column, T freeValue)
        {
            for (var row = 0; row < shape.Height; row++)
            {
                var value = shape.GetCellValue(column, row);
                var isFree = value.Equals(freeValue);
                if (!isFree) return true;
            }
            return false;
        }
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool CanPlaceItem<T>(this in ValueGridShape<T>.ReadOnly container, ImmutableGridShape item, GridPosition pos, T freeValue = default) where T : unmanaged, System.IEquatable<T>
    {
        // Bounds check
        if (pos.X < 0 || pos.Y < 0 || pos.X + item.Width > container.Width || pos.Y + item.Height > container.Height)
            return false;

        // Check each bit of the shape against the grid
        for (var sy = 0; sy < item.Height; sy++)
        for (var sx = 0; sx < item.Width; sx++)
        {
            var shapePos = new GridPosition(sx, sy);
            if (item.GetCellValue(shapePos))
            {
                var gridPos = new GridPosition(pos.X + sx, pos.Y + sy);
                if (container.GetCellValue(gridPos).Equals(freeValue))
                    return false;
            }
        }

        return true;
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static (GridPosition position, RotationDegree rotation) FindFirstFitWithFreeRotation<T>(this in ValueGridShape<T>.ReadOnly container, ImmutableGridShape item) where T : unmanaged, System.IEquatable<T>
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
            _ => throw new System.NotImplementedException()
        };

        return (position, rotation);
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool CheckShapeCells<T, TData>(this in ValueGridShape<T>.ReadOnly grid, ImmutableGridShape shape, GridPosition position, System.Func<GridPosition, T, TData, bool> cellPredicate, TData data) where T : unmanaged, System.IEquatable<T>
    {
        for (int y = 0; y < shape.Height; y++)
        {
            for (int x = 0; x < shape.Width; x++)
            {
                if (shape[x, y]) // Shape cell is occupied
                {
                    var gridPos = new GridPosition(position.X + x, position.Y + y);

                    // Check bounds
                    if (!grid.Contains(gridPos))
                        return false;

                    // Check predicate
                    if (!cellPredicate(gridPos, grid[gridPos], data))
                        return false;
                }
            }
        }

        return true;
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static GridPosition FindFirstFitWithFixedRotation<T>(this ValueGridShape<T>.ReadOnly grid, ImmutableGridShape item, T freeValue = default) where T : unmanaged, System.IEquatable<T>
    {
        var maxY = grid.Height - item.Height + 1;
        var maxX = grid.Width - item.Width + 1;

        for (var y = 0; y < maxY; y++)
        for (var x = 0; x < maxX; x++)
            if (CanPlaceItem(grid, item, new GridPosition(x, y), freeValue))
                return new GridPosition(x, y);

        return GridPosition.Invalid;
    }

#endregion
    
#region ImmutableGridShape

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static int GetIndex(this in ImmutableGridShape shape, GridPosition pos) => GetIndex(shape, pos.X, pos.Y);

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static int GetIndex(this in ImmutableGridShape shape, int x, int y) => y * shape.Width + x;

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool GetCellValue(this in ImmutableGridShape shape, GridPosition pos) => shape[pos];

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool GetCellValue(this in ImmutableGridShape shape, int x, int y) => GetCellValue(shape, new GridPosition(x, y));

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static int Size(this in ImmutableGridShape shape) => shape.Width * shape.Height;

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool IsEmpty(this in ImmutableGridShape shape) => shape.Width <= 0 || shape.Height <= 0;

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool Contains(this in ImmutableGridShape shape, GridPosition pos) => Contains(shape, pos.X, pos.Y);

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool Contains(this in ImmutableGridShape shape, int x, int y) => x >= 0 && x < shape.Width && y >= 0 && y < shape.Height;

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static int CountValue(this in ImmutableGridShape shape, bool target)
    {
        return Count(shape, (v, t) => System.Collections.Generic.EqualityComparer<bool>.Default.Equals(v, t), target);
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static int CountWhere(this in ImmutableGridShape shape, System.Func<bool, bool> predicate)
    {
        return Count(shape, (value, p) => p(value), predicate);
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static int Count<TCaptureData>(this in ImmutableGridShape shape, System.Func<bool, TCaptureData, bool> predicate, TCaptureData data)
    {
        int count = 0;
        var size = shape.Width * shape.Height;
        for (int i = 0; i < size; i++)
        {
            if (predicate(shape[i], data))
                count++;
        }
        return count;
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool Any(this in ImmutableGridShape shape, System.Func<bool, bool> predicate)
    {
        return Any(shape, (v, p) => p(v), predicate);
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool Any<TCaptureData>(this in ImmutableGridShape shape, System.Func<bool, TCaptureData, bool> predicate, TCaptureData data)
    {
        var size = shape.Width * shape.Height;
        for (int i = 0; i < size; i++)
        {
            if (predicate(shape[i], data))
                return true;
        }
        return false;
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool All(this in ImmutableGridShape shape, System.Func<bool, bool> predicate)
    {
        return All(shape, (v, p) => p(v), predicate);
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool All<TCaptureData>(this in ImmutableGridShape shape, System.Func<bool, TCaptureData, bool> predicate, TCaptureData data)
    {
        var size = shape.Width * shape.Height;
        for (int i = 0; i < size; i++)
        {
            if (!predicate(shape[i], data))
                return false;
        }
        return true;
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool IsTrimmed(this in ImmutableGridShape shape, bool freeValue = default)
    {
        // Empty shapes are considered trimmed
        if (shape.Width == 0 || shape.Height == 0)
            return true;

        // A shape is trimmed when all four borders have at least one occupied cell
        return HasOccupiedCellInRow(shape, 0, freeValue) &&                    // Top row
               HasOccupiedCellInRow(shape, shape.Height - 1, freeValue) &&     // Bottom row
               HasOccupiedCellInColumn(shape, 0, freeValue) &&                 // Left column
               HasOccupiedCellInColumn(shape, shape.Width - 1, freeValue);     // Right column

        static bool HasOccupiedCellInRow(in ImmutableGridShape shape, int row, bool freeValue)
        {
            for (var column = 0; column < shape.Width; column++)
            {
                var value = shape.GetCellValue(column, row);
                var isFree = value.Equals(freeValue);
                if (!isFree) return true;
            }
            return false;
        }

        static bool HasOccupiedCellInColumn(in ImmutableGridShape shape, int column, bool freeValue)
        {
            for (var row = 0; row < shape.Height; row++)
            {
                var value = shape.GetCellValue(column, row);
                var isFree = value.Equals(freeValue);
                if (!isFree) return true;
            }
            return false;
        }
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool CanPlaceItem(this in ImmutableGridShape container, ImmutableGridShape item, GridPosition pos, bool freeValue = default)
    {
        // Bounds check
        if (pos.X < 0 || pos.Y < 0 || pos.X + item.Width > container.Width || pos.Y + item.Height > container.Height)
            return false;

        // Check each bit of the shape against the grid
        for (var sy = 0; sy < item.Height; sy++)
        for (var sx = 0; sx < item.Width; sx++)
        {
            var shapePos = new GridPosition(sx, sy);
            if (item.GetCellValue(shapePos))
            {
                var gridPos = new GridPosition(pos.X + sx, pos.Y + sy);
                if (container.GetCellValue(gridPos).Equals(freeValue))
                    return false;
            }
        }

        return true;
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static (GridPosition position, RotationDegree rotation) FindFirstFitWithFreeRotation(this in ImmutableGridShape container, ImmutableGridShape item)
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
            _ => throw new System.NotImplementedException()
        };

        return (position, rotation);
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static bool CheckShapeCells<TData>(this in ImmutableGridShape grid, ImmutableGridShape shape, GridPosition position, System.Func<GridPosition, bool, TData, bool> cellPredicate, TData data)
    {
        for (int y = 0; y < shape.Height; y++)
        {
            for (int x = 0; x < shape.Width; x++)
            {
                if (shape[x, y]) // Shape cell is occupied
                {
                    var gridPos = new GridPosition(position.X + x, position.Y + y);

                    // Check bounds
                    if (!grid.Contains(gridPos))
                        return false;

                    // Check predicate
                    if (!cellPredicate(gridPos, grid[gridPos], data))
                        return false;
                }
            }
        }

        return true;
    }

    [JetBrains.Annotations.Pure, JetBrains.Annotations.MustUseReturnValue]
    public static GridPosition FindFirstFitWithFixedRotation(this ImmutableGridShape grid, ImmutableGridShape item, bool freeValue = default)
    {
        var maxY = grid.Height - item.Height + 1;
        var maxX = grid.Width - item.Width + 1;

        for (var y = 0; y < maxY; y++)
        for (var x = 0; x < maxX; x++)
            if (CanPlaceItem(grid, item, new GridPosition(x, y), freeValue))
                return new GridPosition(x, y);

        return GridPosition.Invalid;
    }

#endregion
    }

