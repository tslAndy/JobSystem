namespace JobSystem;

public unsafe struct JobHandle
{
    private JobId* buffer;
    private int length;

    public JobHandle(int length) => buffer = JobSysServ.Arena.Allocate<JobId>(this.length = length);

    public int Length => length;
    public ref JobId this[int index]
    {
        get
        {
            if (0 <= index && index < length)
                return ref buffer[index];
            throw new ArgumentOutOfRangeException();
        }
    }

    public static JobHandle operator +(JobHandle left, JobHandle right)
    {
        JobId* buffer = JobSysServ.Arena.Allocate<JobId>(left.length + right.length);
        JobSysServ.Arena.Copy<JobId>(left.buffer, buffer, left.length);
        JobSysServ.Arena.Copy<JobId>(right.buffer, buffer + left.length, right.length);

        return new JobHandle
        {
            buffer = buffer,
            length = left.length + right.length
        };
    }
}


