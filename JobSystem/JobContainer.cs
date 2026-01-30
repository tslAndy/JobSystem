namespace JobSystem;

unsafe class JobContainer<T> : Container where T : unmanaged, IJob
{
    private Bag<T> jobs;

    public JobHandle AddJob(T job, JobHandle handle)
    {
        jobs.Add(job);
        descendants.Add(new Bag<JobId>());
        degree.Add(handle.Length);

        JobHandle result = new JobHandle(1);
        result[0] = new JobId(typeof(T).TypeHandle.Value, jobs.Count - 1);
        return result;
    }

    public override void Schedule(int jobIndex)
    {
        degree[jobIndex] = 1;
        JobData jobData = new JobData(jobs.GetPtr(jobIndex), degree.GetPtr(jobIndex));
        ThreadPool.QueueUserWorkItem<JobData>(
                x =>
                {
                    x.job->Execute();
                    (*x.degree)--;
                },
                jobData,
                true
        );
    }

    protected override void ResetInternal()
    {
        jobs = default;
    }

    private readonly struct JobData
    {
        public readonly T* job;
        public readonly int* degree;

        public JobData(T* job, int* degree)
        {
            this.job = job;
            this.degree = degree;
        }
    }
}


