using System.Diagnostics.CodeAnalysis;

namespace Unity.Mathematics;

[SuppressMessage("Usage", "CA2225:Operator overloads have named alternates")]
public readonly record struct int2(int x, int y)
{
    public readonly int x = x;
    public readonly int y = y;

    public static int2 zero => new(0, 0);

    public static int2 operator +(int2 a, int2 b) => new(a.x + b.x, a.y + b.y);
    public static int2 operator -(int2 a, int2 b) => new(a.x - b.x, a.y - b.y);
    public static int2 operator -(int2 a) => new(-a.x, -a.y);
    public static int2 operator *(int2 a, int scalar) => new(a.x * scalar, a.y * scalar);
    public static int2 operator *(int scalar, int2 a) => new(a.x * scalar, a.y * scalar);
}
