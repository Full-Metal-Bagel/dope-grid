using System.Runtime.InteropServices;

namespace DopeGrid;

[StructLayout(LayoutKind.Sequential, Pack = 8, Size = 8)]
public readonly record struct Bytes8
{
    public const int Size = 8;
    private readonly long _l0;
}

[StructLayout(LayoutKind.Sequential, Pack = 8, Size = 32)]
public readonly record struct Bytes32
{
    public const int Size = 32;
    private readonly long _l0;
    private readonly long _l1;
    private readonly long _l2;
    private readonly long _l3;
}
