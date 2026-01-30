namespace JobSystem;

public readonly struct JobId
{
    public readonly IntPtr type;
    public readonly int index;

    public JobId(nint type, int index)
    {
        this.type = type;
        this.index = index;
    }
}


