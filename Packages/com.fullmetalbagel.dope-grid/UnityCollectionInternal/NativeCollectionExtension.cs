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
        public static UnsafeBitArray.ReadOnly CreateReadOnlyUnsafeBitArray(ulong* ptr, int length) => new(ptr, length);

        [Pure, MustUseReturnValue]
        public static UnsafeBitArray* GetUnsafeBitArrayPtr(this NativeBitArray bits) => bits.m_BitArray;

        [Pure, MustUseReturnValue]
        public static int CalculateHashCode(this NativeBitArray bits)
        {
            if (!bits.IsCreated)
                return 0;
            return bits.GetReadOnlyUnsafeBitArray().CalculateHashCode();
        }

        [Pure, MustUseReturnValue]
        public static int CalculateHashCode(this NativeBitArray.ReadOnly bits)
        {
            return bits.GetReadOnlyUnsafeBitArray().CalculateHashCode();
        }

        [Pure, MustUseReturnValue]
        public static int CalculateHashCode(this UnsafeBitArray bits)
        {
            return bits.AsReadOnly().CalculateHashCode();
        }

        [Pure, MustUseReturnValue]
        public static int CalculateHashCode(this UnsafeBitArray.ReadOnly bits)
        {
            if (bits.Ptr == null || bits.Length == 0)
                return 0;

            // Use FNV-1a hash algorithm for good distribution
            unchecked
            {
                const int fnvPrime = 16777619;
                int hash = (int)2166136261;

                // Hash complete ulong blocks
                int fullLongs = bits.Length / 64;
                for (int i = 0; i < fullLongs; i++)
                {
                    ulong value = bits.Ptr[i];

                    // Hash each byte of the ulong
                    for (int j = 0; j < 8; j++)
                    {
                        hash = (hash ^ (byte)(value >> (j * 8))) * fnvPrime;
                    }
                }

                // Hash remaining bits if any
                int remainingBits = bits.Length % 64;
                if (remainingBits > 0)
                {
                    ulong lastValue = bits.Ptr[fullLongs];
                    // Mask to only consider valid bits
                    ulong mask = (1UL << remainingBits) - 1;
                    lastValue &= mask;

                    // Hash the valid bytes of the last partial ulong
                    int bytesToHash = (remainingBits + 7) / 8;
                    for (int j = 0; j < bytesToHash; j++)
                    {
                        hash = (hash ^ (byte)(lastValue >> (j * 8))) * fnvPrime;
                    }
                }

                // Include length in the hash to differentiate arrays with trailing zeros
                hash = (hash ^ bits.Length) * fnvPrime;

                return hash;
            }
        }

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
