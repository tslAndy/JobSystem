namespace NatColls;

public readonly unsafe struct BufferRO<T> where T : unmanaged
{
    private readonly T* buffer;
    private readonly int length;

    public BufferRO(T* buffer, int length)
    {
        this.buffer = buffer;
        this.length = length;
    }

    public int Length => length;
    public T this[int index]
    {
        get
        {
            if (0 <= index && index < length)
                return buffer[index];
            throw new ArgumentOutOfRangeException();
        }
    }

    public ReadOnlySpan<T> ReadOnlySpan => new ReadOnlySpan<T>(buffer, length);
}

