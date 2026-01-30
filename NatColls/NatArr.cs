using System.Runtime.InteropServices;

namespace NatColls;

public unsafe struct NatArr<T> : IDisposable where T : unmanaged
{
    private T* buffer;
    private int length;

    public NatArr(int length)
    {
        this.buffer = (T*)NativeMemory.AlignedAlloc((nuint)(length * sizeof(T)), 4);
        this.length = length;
    }

    public int Length => length;
    public ref T this[int index]
    {
        get
        {
            if (0 <= index && index < length)
                return ref buffer[index];
            throw new ArgumentOutOfRangeException();
        }
    }

    public BufferRO<T> BufferRO => new BufferRO<T>(buffer, length);
    public BufferRW<T> BufferRW => new BufferRW<T>(buffer, length);

    public void Dispose() => NativeMemory.AlignedFree(buffer);
}

/*
 *
 * индексаторы NatArr и DBuff должны использоваться только при инициализации 
 * в самих работах используются BufferRO BufferRW
 *
 * */
