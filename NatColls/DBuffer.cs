using System.Runtime.InteropServices;

namespace NatColls;

public unsafe struct DBuffer<T> : IDisposable
    where T : unmanaged
{
    private T* current,
        next;
    private int length;

    public DBuffer(int length)
    {
        current = (T*)NativeMemory.AlignedAlloc((nuint)(2 * length * sizeof(T)), 4);
        next = current + length;
        this.length = length;
    }

    public int Length => length;

    public void Swap()
    {
        T* temp = current;
        current = next;
        next = temp;
    }

    public ref T this[int index]
    {
        get
        {
            if (0 <= index && index < length)
                return ref current[index];
            throw new ArgumentOutOfRangeException();
        }
    }

    public BufferRO<T> BufferRO => new BufferRO<T>(current, length);
    public BufferRW<T> BufferRW => new BufferRW<T>(next, length);

    public void Dispose() => NativeMemory.AlignedFree(current < next ? current : next);
}
