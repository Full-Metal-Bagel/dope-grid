using System;
using System.Runtime.InteropServices;

namespace DopeGrid;

public interface ISize
{
    IntPtr HeadPtr { get; }
    int Size { get; }
}

[StructLayout(LayoutKind.Sequential, Pack = 8, Size = 8)]
public readonly record struct Bytes8 : ISize
{
    public int Size => 8;
    private readonly long _l0;

    public unsafe IntPtr HeadPtr
    {
        get
        {
            fixed (void* ptr = &_l0)
                return new IntPtr(ptr);
        }
    }
}

[StructLayout(LayoutKind.Sequential, Pack = 8, Size = 32)]
public readonly record struct Bytes32 : ISize
{
    public int Size => 32;
    private readonly long _l0;
    private readonly long _l1;
    private readonly long _l2;
    private readonly long _l3;

    public unsafe IntPtr HeadPtr
    {
        get
        {
            fixed (void* ptr = &_l0)
                return new IntPtr(ptr);
        }
    }
}
