using System;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace DopeGrid;

public static class Shapes
{
    [SuppressMessage("Naming", "CA1720:Identifier contains type name")]
    [Pure, MustUseReturnValue]
    public static FixedGridShape<ulong> Single()
    {
        var shape = new FixedGridShape<ulong>(1, 1);
        shape[0, 0] = true;
        return shape;
    }

    [Pure, MustUseReturnValue]
    public static FixedGridShape<ulong> Line(int length)
    {
        var shape = new FixedGridShape<ulong>(length, 1);
        for (var i = 0; i < length; i++)
            shape[i, 0] = true;
        return shape;
    }

    [Pure, MustUseReturnValue]
    public static FixedGridShape<ulong> Square(int size)
    {
        var shape = new FixedGridShape<ulong>(size, size);
        for (var y = 0; y < size; y++)
        for (var x = 0; x < size; x++)
            shape[x, y] = true;
        return shape;
    }

    [Pure, MustUseReturnValue]
    public static FixedGridShape<ulong> LShape()
    {
        var shape = new FixedGridShape<ulong>(2, 2);
        shape[0, 0] = true;
        shape[0, 1] = true;
        shape[1, 1] = true;
        return shape;
    }

    [Pure, MustUseReturnValue]
    public static FixedGridShape<ulong> TShape()
    {
        var shape = new FixedGridShape<ulong>(3, 2);
        shape[0, 0] = true;
        shape[1, 0] = true;
        shape[2, 0] = true;
        shape[1, 1] = true;
        return shape;
    }

    [Pure, MustUseReturnValue]
    public static FixedGridShape<ulong> Cross()
    {
        var shape = new FixedGridShape<ulong>(3, 3);
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
        var shape = Single();
        return shape.GetOrCreateImmutable();
    }

    [Pure, MustUseReturnValue]
    public static ImmutableGridShape ImmutableLine(int length)
    {
        var shape = Line(length);
        return shape.GetOrCreateImmutable();
    }

    [Pure, MustUseReturnValue]
    public static ImmutableGridShape ImmutableSquare(int size)
    {
        var shape = Square(size);
        return shape.GetOrCreateImmutable();
    }

    [Pure, MustUseReturnValue]
    public static ImmutableGridShape ImmutableLShape()
    {
        var shape = LShape();
        return shape.GetOrCreateImmutable();
    }

    [Pure, MustUseReturnValue]
    public static ImmutableGridShape ImmutableTShape()
    {
        var shape = TShape();
        return shape.GetOrCreateImmutable();
    }

    [Pure, MustUseReturnValue]
    public static ImmutableGridShape ImmutableCross()
    {
        var shape = Cross();
        return shape.GetOrCreateImmutable();
    }

    public static unsafe TResult UnsafeProcessShape<TCaptureData, TResult>(int width, int height, TCaptureData data, Func<UnsafeBitsGridShape, TCaptureData, TResult> processor)
    {
        if (processor is null) throw new ArgumentNullException(nameof(processor));

        var size = width * height;

        if (SpanBitArrayUtility.ByteCount(size) <= 512)
        {
            var buffer = stackalloc byte[size];
            var @unsafe = new UnsafeBitsGridShape(width, height, buffer);
            return processor(@unsafe, data);
        }

        {
            using var shape = new GridShape(width, height);
            using var @unsafe = shape.AsUnsafe();
            return processor(@unsafe, data);
        }
    }
}
