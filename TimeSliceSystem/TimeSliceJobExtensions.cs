namespace TimeSliceSystem;

public static class TimeSliceJobExtensions
{
    public static TimeSliceJobHandle Schedule<T>(this T job, int length, int kernelsCount, int framesCount) where T : unmanaged, ITimeSliceJob
    {
        return TimeSliceJobManager.AddJob(job, length, kernelsCount, framesCount);
    }
}


