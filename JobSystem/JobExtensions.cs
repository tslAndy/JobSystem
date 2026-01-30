namespace JobSystem;

public static class JobExtensions
{
    public static JobHandle Schedule<T>(this T job, JobHandle handle = default) where T : unmanaged, IJob
    {
        return JobManager.AddJob(job, handle);
    }

    public static JobHandle Schedule<T>(this T job, int length, int kernelCount, JobHandle handle = default) where T : unmanaged, IJobParallel
    {
        return JobManager.AddJobParallel(job, length, kernelCount, handle);
    }
}


