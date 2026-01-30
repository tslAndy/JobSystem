using System.Runtime.InteropServices;

namespace Allocation.Arena;

unsafe struct Bag<T> : IDisposable where T : unmanaged
{
    private T* buffer;
    private nuint count, capacity;

    public Bag(int capacity)
    {
        this.capacity = (nuint)(capacity * sizeof(T));
        this.buffer = (T*)NativeMemory.AlignedAlloc(this.capacity, 4);
    }

    public void Add(T elem)
    {
        if (count == capacity)
        {
            capacity *= 2;
            this.buffer = (T*)NativeMemory.AlignedRealloc(buffer, capacity, 4);
        }
        buffer[count++] = elem;
    }

    public nuint Count => count;
    public ref T this[nuint index] => ref buffer[index];

    public void Dispose() => NativeMemory.AlignedFree(buffer);
}

