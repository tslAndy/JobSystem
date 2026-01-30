namespace TimeSliceSystem;

abstract class Container : IDisposable
{
    public abstract void Dispose();
    public abstract void ScheduleJobs();
}


