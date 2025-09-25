using JetBrains.Annotations;
using UnityEngine;

namespace DopeGrid;

public enum RotationDegree
{
    None = 0,
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
    public static Vector2 GetRotationOffset(this RotationDegree rotation, Vector2 size)
    {
        var w = size.x;
        var h = size.y;
        return rotation switch
        {
            RotationDegree.None => Vector2.zero,
            RotationDegree.Clockwise90 => new Vector2(h, 0f),
            RotationDegree.Clockwise180 => new Vector2(w, -h),
            RotationDegree.Clockwise270 => new Vector2(0f, -w),
            _ => Vector2.zero
        };
    }

    [Pure, MustUseReturnValue]
    public static Vector2 CalculateRotatedSize(this RotationDegree rotation, Vector2 size)
    {
        return rotation is RotationDegree.Clockwise90 or RotationDegree.Clockwise270
            ? new Vector2(size.y, size.x)
            : size;
    }

    public static void ApplyToRectTransform(this RotationDegree rotation, RectTransform transform, Vector2 size)
    {
        var angleZ = rotation.GetZRotation();
        transform.sizeDelta = rotation.CalculateRotatedSize(size);
        transform.localEulerAngles = new Vector3(0f, 0f, angleZ);
    }
}
