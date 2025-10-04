using System;
using Unity.Collections;

namespace DopeGrid.Native;

public static class NativeExtension
{
    public static bool ArraysEqual<T>(this NativeArray<T>.ReadOnly container, NativeArray<T>.ReadOnly other)
        where T : unmanaged, IEquatable<T>
    {
        if (container.Length != other.Length)
            return false;

        for (int i = 0; i != container.Length; i++)
        {
            if (!container[i].Equals(other[i]))
                return false;
        }

        return true;
    }
}
