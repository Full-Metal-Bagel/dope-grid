using System;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace DopeGrid;

public static class Shapes
{
    [SuppressMessage("Naming", "CA1720:Identifier contains type name")]
    [Pure, MustUseReturnValue]
    public static GridShape Single()
    {
        var shape = new GridShape(1, 1);
        shape[0, 0] = true;
        return shape;
    }

    [Pure, MustUseReturnValue]
    public static GridShape Line(int length)
    {
        var shape = new GridShape(length, 1);
        for (var i = 0; i < length; i++)
            shape[i, 0] = true;
        return shape;
    }

    [Pure, MustUseReturnValue]
    public static GridShape Square(int size)
    {
        var shape = new GridShape(size, size);
        for (var y = 0; y < size; y++)
        for (var x = 0; x < size; x++)
            shape[x, y] = true;
        return shape;
    }

    [Pure, MustUseReturnValue]
    public static GridShape LShape()
    {
        var shape = new GridShape(2, 2);
        shape[0, 0] = true;
        shape[0, 1] = true;
        shape[1, 1] = true;
        return shape;
    }

    [Pure, MustUseReturnValue]
    public static GridShape TShape()
    {
        var shape = new GridShape(3, 2);
        shape[0, 0] = true;
        shape[1, 0] = true;
        shape[2, 0] = true;
        shape[1, 1] = true;
        return shape;
    }

    [Pure, MustUseReturnValue]
    public static GridShape Cross()
    {
        var shape = new GridShape(3, 3);
        shape[1, 0] = true;
        shape[0, 1] = true;
        shape[1, 1] = true;
        shape[2, 1] = true;
        shape[1, 2] = true;
        return shape;
    }

    // Immutable shape factory methods
    [Pure, MustUseReturnValue]
    public static ImmutableGridShape ImmutableSingle()
    {
        using var shape = Single();
        return shape.GetOrCreateImmutable();
    }

    [Pure, MustUseReturnValue]
    public static ImmutableGridShape ImmutableLine(int length)
    {
        using var shape = Line(length);
        return shape.GetOrCreateImmutable();
    }

    [Pure, MustUseReturnValue]
    public static ImmutableGridShape ImmutableSquare(int size)
    {
        using var shape = Square(size);
        return shape.GetOrCreateImmutable();
    }

    [Pure, MustUseReturnValue]
    public static ImmutableGridShape ImmutableLShape()
    {
        using var shape = LShape();
        return shape.GetOrCreateImmutable();
    }

    [Pure, MustUseReturnValue]
    public static ImmutableGridShape ImmutableTShape()
    {
        using var shape = TShape();
        return shape.GetOrCreateImmutable();
    }

    [Pure, MustUseReturnValue]
    public static ImmutableGridShape ImmutableCross()
    {
        using var shape = Cross();
        return shape.GetOrCreateImmutable();
    }

    [Pure, MustUseReturnValue]
    public static ImmutableGridShape GetRotatedShape(this ImmutableGridShape shape, RotationDegree rotation)
    {
        return rotation switch
        {
            RotationDegree.None => shape,
            RotationDegree.Clockwise90 => shape.Rotate90(),
            RotationDegree.Clockwise180 => shape.Rotate90().Rotate90(),
            RotationDegree.Clockwise270 => shape.Rotate90().Rotate90().Rotate90(),
            _ => throw new ArgumentOutOfRangeException(nameof(rotation), rotation, null)
        };
    }

    public static TResult UnsafeProcessShape<TCaptureData, TResult>(int width, int height, TCaptureData data, Func<UnsafeBitsGridShape, TCaptureData, TResult> processor)
    {
        if (processor is null) throw new ArgumentNullException(nameof(processor));

        var size = width * height;

        if (SpanBitArrayUtility.ByteCount(size) <= 8)
        {
            var shape = new FixedGridShape<Bytes8>(width, height);
            return processor(shape.AsUnsafe(), data);
        }
        if (SpanBitArrayUtility.ByteCount(size) <= 32)
        {
            var shape = new FixedGridShape<Bytes32>(width, height);
            return processor(shape.AsUnsafe(), data);
        }

        {
            using var shape = new GridShape(width, height);
            using var @unsafe = shape.AsUnsafe();
            return processor(@unsafe, data);
        }
    }
}
