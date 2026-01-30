namespace JobSystem;

unsafe struct Bag<T> where T : unmanaged
{
    private T* buffer;
    private int count, length;

    public void Add(T elem)
    {
        if (count < length)
        {
            buffer[count++] = elem;
            return;
        }

        int new_length = Math.Max(length * 2, 1);
        T* new_buffer = JobSysServ.Arena.Allocate<T>(new_length);
        JobSysServ.Arena.Copy<T>(buffer, new_buffer, count);

        length = new_length;
        buffer = new_buffer;
        buffer[count++] = elem;
    }

    public int Count => count;
    public T* GetPtr(int index)
    {
        if (0 <= index && index < count)
            return buffer + index;
        throw new ArgumentOutOfRangeException();
    }
    public ref T this[int index]
    {
        get
        {
            if (0 <= index && index < count)
                return ref buffer[index];
            throw new ArgumentOutOfRangeException();
        }
    }
}


