using System.Runtime.InteropServices;

namespace Allocation.Arena;

unsafe struct Block : IDisposable
{
    private IntPtr buffer;
    private nuint fill, capacity;

    public Block(nuint capacity) => buffer = new IntPtr(NativeMemory.AlignedAlloc(this.capacity = capacity, 4));

    public bool TryAllocate(nuint length, out void* ptr)
    {
        ptr = null;

        length += length % 4;
        if (fill + length > capacity)
            return false;

        ptr = (buffer + (nint)fill).ToPointer();
        fill += length;
        return true;
    }

    public void Reset() => fill = 0;
    public void Dispose() => NativeMemory.AlignedFree(buffer.ToPointer());
}

