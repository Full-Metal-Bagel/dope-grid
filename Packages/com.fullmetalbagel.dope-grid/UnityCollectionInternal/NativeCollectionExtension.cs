using JetBrains.Annotations;
using Unity.Collections.LowLevel.Unsafe;

namespace Unity.Collections
{
    public static unsafe class NativeCollectionExtension
    {
        [Pure, MustUseReturnValue]
        public static UnsafeBitArray.ReadOnly GetReadOnlyUnsafeBitArray(this NativeBitArray bits) => bits.m_BitArray->AsReadOnly();

        [Pure, MustUseReturnValue]
        public static UnsafeBitArray.ReadOnly GetReadOnlyUnsafeBitArray(this NativeBitArray.ReadOnly bits) => bits.m_BitArray;

        [Pure, MustUseReturnValue]
        public static UnsafeBitArray* GetUnsafeBitArrayPtr(this NativeBitArray bits) => bits.m_BitArray;

        public static bool SequenceEquals(this UnsafeBitArray.ReadOnly bits1, UnsafeBitArray.ReadOnly bits2)
        {
            if (bits1.Ptr == bits2.Ptr)
                return true;

            if (bits1.Ptr == null || bits2.Ptr == null)
                return false;

            if (bits1.Length != bits2.Length)
                return false;

            int length = bits1.Length;

            // Compare full 64-bit words using native memcmp
            long fullWords = length / 64;
            long bytesToCompare = fullWords * 8;
            if (bytesToCompare > 0 && UnsafeUtility.MemCmp(bits1.Ptr, bits2.Ptr, bytesToCompare) != 0)
                return false;

            // Compare remaining bits (mask the tail word)
            int rem = length % 64;
            if (rem == 0)
                return true;

            ulong a = bits1.Ptr[fullWords];
            ulong b = bits2.Ptr[fullWords];
            ulong mask = (1UL << rem) - 1UL;
            return ((a ^ b) & mask) == 0;
        }
    }
}
