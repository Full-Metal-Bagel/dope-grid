using System;
using System.Runtime.InteropServices;

namespace DopeGrid;

// TODO: better be `ref struct` with `interface`
public readonly unsafe record struct UnsafeBitsGridShape : IGridShape<bool>, IReadOnlyBitsGridShape, IDisposable
{
    public int Width { get; }
    public int Height { get; }
    private readonly IntPtr _ptr;
    private readonly IntPtr _gcHandle;
    public int Size => Width * Height;
    private SpanBitArray Bits => new(new Span<byte>(_ptr.ToPointer(), SpanBitArrayUtility.ByteCount(Size)), Size);
    public ReadOnlySpanBitArray ReadOnlyBits => Bits;

    public bool this[int x, int y]
    {
        get => Bits.Get(this.GetIndex(x, y));
        set => Bits.Set(this.GetIndex(x, y), value);
    }
    public bool IsOccupied(int x, int y) => this[x, y];

    public UnsafeBitsGridShape(int width, int height, IntPtr bitsPtr)
    {
        Width = width;
        Height = height;
        _ptr = bitsPtr;
        _gcHandle = IntPtr.Zero;
    }

    public UnsafeBitsGridShape(int width, int height, byte[] bits)
    {
        Width = width;
        Height = height;
        var handle = GCHandle.Alloc(bits, GCHandleType.Pinned);
        _ptr = handle.AddrOfPinnedObject();
        _gcHandle = GCHandle.ToIntPtr(handle);
    }

    public void Dispose()
    {
        if (_gcHandle != IntPtr.Zero)
        {
            GCHandle.FromIntPtr(_gcHandle).Free();
        }
    }
}
