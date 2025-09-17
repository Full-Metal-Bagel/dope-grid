using Unity.Collections;

namespace DopeGrid.Native;

public static class NativeBitArrayExtensions
{
    public static bool SequenceEquals(this NativeBitArray bits1, NativeBitArray bits2)
    {
        if (!bits1.IsCreated && !bits2.IsCreated)
            return true;

        if (!bits1.IsCreated || !bits2.IsCreated)
            return false;

        var unsafeBits1 = bits1.GetReadOnlyUnsafeBitArray();
        var unsafeBits2 = bits2.GetReadOnlyUnsafeBitArray();
        return unsafeBits1.SequenceEquals(unsafeBits2);
    }

    public static bool SequenceEquals(this NativeBitArray.ReadOnly bits1, NativeBitArray.ReadOnly bits2)
    {
        var unsafeBits1 = bits1.GetReadOnlyUnsafeBitArray();
        var unsafeBits2 = bits2.GetReadOnlyUnsafeBitArray();
        return unsafeBits1.SequenceEquals(unsafeBits2);
    }
}
