using System;
using JetBrains.Annotations;

namespace DopeGrid;

public enum RotationDegree
{
    None = 0,
    Clockwise0 = 0,
    Clockwise90 = 1,
    Clockwise180 = 2,
    Clockwise270 = 3
}

public static class RotationDegreeExtensions
{
    [Pure, MustUseReturnValue]
    public static RotationDegree GetNextClockwiseRotation(this RotationDegree current)
    {
        return current switch
        {
            RotationDegree.None => RotationDegree.Clockwise90,
            RotationDegree.Clockwise90 => RotationDegree.Clockwise180,
            RotationDegree.Clockwise180 => RotationDegree.Clockwise270,
            RotationDegree.Clockwise270 => RotationDegree.None,
            _ => RotationDegree.None
        };
    }

    [Pure, MustUseReturnValue]
    public static RotationDegree Rotate(this RotationDegree current, RotationDegree rotation)
    {
        return (RotationDegree)(((int)current + (int)rotation) % 4);
    }

    [Pure, MustUseReturnValue]
    public static RotationDegree GetPreviousClockwiseRotation(this RotationDegree current)
    {
        return current switch
        {
            RotationDegree.None => RotationDegree.Clockwise270,
            RotationDegree.Clockwise90 => RotationDegree.None,
            RotationDegree.Clockwise180 => RotationDegree.Clockwise90,
            RotationDegree.Clockwise270 => RotationDegree.Clockwise180,
            _ => RotationDegree.None
        };
    }

    [Pure, MustUseReturnValue]
    public static float GetZRotation(this RotationDegree rotation) => rotation switch
    {
        RotationDegree.None => 0f,
        RotationDegree.Clockwise90 => -90f,
        RotationDegree.Clockwise180 => -180f,
        RotationDegree.Clockwise270 => -270f,
        _ => 0f
    };

    [Pure, MustUseReturnValue]
    public static (float offsetX, float offsetY) GetRotationOffset(this RotationDegree rotation, float width, float height)
    {
        return rotation switch
        {
            RotationDegree.None => (0f, 0f),
            RotationDegree.Clockwise90 => (height, 0f),
            RotationDegree.Clockwise180 => (width, -height),
            RotationDegree.Clockwise270 => (0f, -width),
            _ => throw new NotSupportedException()
        };
    }

    [Pure, MustUseReturnValue]
    public static (float width, float height) CalculateRotatedSize(this RotationDegree rotation, float width, float height)
    {
        return rotation is RotationDegree.Clockwise90 or RotationDegree.Clockwise270
            ? (height, width)
            : (width, height);
    }

    [Pure, MustUseReturnValue]
    public static (int width, int height) CalculateRotatedSize(this RotationDegree rotation, int width, int height)
    {
        return rotation is RotationDegree.Clockwise90 or RotationDegree.Clockwise270 ? (height, width) : (width, height);
    }
}
