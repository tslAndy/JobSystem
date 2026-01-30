namespace TimeSliceSystem;

unsafe struct Bag<T> : IDisposable where T : unmanaged
{
    private T* buffer;
    private int count, capacity;

    public Bag(int capacity)
    {
        this.capacity = capacity;
        this.buffer = TimeSliceSysServ.Pool.Allocate<T>(capacity);
    }

    public void Add(T elem)
    {
        if (count >= capacity)
        {
            capacity *= 2;
            buffer = TimeSliceSysServ.Pool.Realloc<T>(buffer, capacity);
        }

        buffer[count++] = elem;
    }

    public void RemoveRange(int index, int length)
    {
        if (index < 0 || index + length > count)
            throw new ArgumentOutOfRangeException();

        T* ptr = buffer + index;
        int offset = count - length - index;
        TimeSliceSysServ.Pool.Copy(ptr + length, ptr, offset);
        count -= length;
    }

    public void Remove(int index)
    {
        if (index < 0 || index >= count)
            throw new ArgumentOutOfRangeException();

        int offset = count - index - 1;
        TimeSliceSysServ.Pool.Copy(buffer + index + 1, buffer + index, offset);
        count--;
    }

    public void Insert(T elem, int index)
    {
        if (index < 0 || index >= count)
            throw new ArgumentOutOfRangeException();

        if (count >= capacity)
        {
            capacity *= 2;
            buffer = TimeSliceSysServ.Pool.Realloc(buffer, capacity);
        }

        int length = count - index;
        TimeSliceSysServ.Pool.Copy(buffer + index, buffer + index + 1, length);
        count++;
    }

    public void Fill(T val)
    {
        for (int i = 0; i < count; i++)
            buffer[i] = val;
    }

    public int Count => count;

    public ref T this[int index]
    {
        get
        {
            if (0 <= index && index < count)
                return ref buffer[index];
            throw new ArgumentOutOfRangeException();
        }
    }

    public T* GetPtr(int index)
    {
        if (0 <= index && index < count)
            return buffer + index;
        throw new ArgumentOutOfRangeException();
    }

    public void Dispose() => TimeSliceSysServ.Pool.Deallocate(buffer);
}


