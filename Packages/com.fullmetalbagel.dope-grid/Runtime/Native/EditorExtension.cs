using JetBrains.Annotations;
using Unity.Collections;

namespace DopeGrid.Native;

public static class EditorExtension
{
    [Pure, MustUseReturnValue]
    public static GridShape ToGridShape(this EditorGridShape editorShape, Allocator allocator = Allocator.Temp)
    {
        var shape = new GridShape(editorShape.Width, editorShape.Height, allocator);

        for (int y = 0; y < editorShape.Height; y++)
        {
            for (int x = 0; x < editorShape.Width; x++)
            {
                var index = y * editorShape.Width + x;
                if (index < editorShape.Shape.Length)
                {
                    shape[x, y] = editorShape.Shape[index];
                }
            }
        }

        return shape;
    }

    [Pure, MustUseReturnValue]
    public static GridShape ToTrimmedGridShape(this EditorGridShape editorShape, Allocator allocator = Allocator.Temp)
    {
        var shape = editorShape.ToGridShape(Allocator.Temp);
        return shape.AsReadOnly().Trim();
    }

    [Pure, MustUseReturnValue]
    public static ImmutableGridShape ToImmutableGridShape(this EditorGridShape editorShape)
    {
        return editorShape.ToTrimmedGridShape(Allocator.Temp).GetOrCreateImmutable();
    }
}
