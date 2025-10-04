#if UNITY_2022_3_OR_NEWER

using System;
using JetBrains.Annotations;

namespace DopeGrid;

[AttributeUsage(AttributeTargets.Field)]
public class EditorGridShapeReferenceImageAttribute : Attribute
{
    public string MemberName { get; }
    public EditorGridShapeReferenceImageAttribute(string memberName) => MemberName = memberName;
}

[Serializable]
public struct EditorGridShape
{
    [field: UnityEngine.SerializeField] public int Width { get; private set; }
    [field: UnityEngine.SerializeField] public int Height { get; private set; }
    [field: UnityEngine.SerializeField] public bool[] Shape { get; private set; }

    [Pure, MustUseReturnValue]
    public GridShape ToGridShape()
    {
        var shape = new GridShape(Width, Height);

        for (int y = 0; y < Height; y++)
        for (int x = 0; x < Width; x++)
        {
            var index = y * Width + x;
            if (index < Shape.Length)
            {
                shape[x, y] = Shape[index];
            }
        }

        return shape;
    }

    [Pure, MustUseReturnValue]
    public GridShape ToTrimmedGridShape()
    {
        using var shape = ToGridShape();
        var (_, _, width, height) = shape.GetTrimmedBound(default(bool));
        var trimmedShape = new GridShape(width, height);
        shape.Trim(trimmedShape, default(bool));
        return trimmedShape;
    }

    [Pure, MustUseReturnValue]
    public ImmutableGridShape ToImmutableGridShape()
    {
        return ToTrimmedGridShape().GetOrCreateImmutable();
    }
}

#endif
