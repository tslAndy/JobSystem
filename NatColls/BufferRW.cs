namespace NatColls;

public readonly unsafe struct BufferRW<T> where T : unmanaged
{
    private readonly T* buffer;
    private readonly int length;

    public BufferRW(T* buffer, int length)
    {
        this.buffer = buffer;
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

    public Span<T> Span => new Span<T>(buffer, length);
}

