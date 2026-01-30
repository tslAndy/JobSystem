namespace JobSystem;

unsafe class JobParallelContainer<T> : Container where T : unmanaged, IJobParallel
{
    private Bag<T> jobs;
    private Bag<Batch> batches;

    public JobHandle AddJob(T job, int length, int kernelsCount, JobHandle handle)
    {
        jobs.Add(job);
        descendants.Add(new Bag<JobId>());
        degree.Add(handle.Length);
        batches.Add(new Batch(length, Math.Min(length, kernelsCount)));

        JobHandle result = new JobHandle(1);
        result[0] = new JobId(typeof(T).TypeHandle.Value, jobs.Count - 1);
        return result;
    }

    public override void Schedule(int jobIndex)
    {
        Batch batch = batches[jobIndex];

        int length = batch.length + (batch.length % batch.kernelsCount);
        int step = length / batch.kernelsCount;

        int* deg = degree.GetPtr(jobIndex);
        (*deg) = batch.kernelsCount;
        T* job = jobs.GetPtr(jobIndex);

        for (int i = 0; i < length; i += step)
        {
            JobData data = new JobData(job, deg, i, Math.Min(batch.length, i + step));
            ThreadPool.QueueUserWorkItem<JobData>(
                    x =>
                    {
                        for (int k = x.start; k < x.end; k++)
                            x.job->Execute(k);
                        Interlocked.Add(ref *x.degree, -1);
                    },
                    data,
                    false
            );
        }
    }

    protected override void ResetInternal()
    {
        jobs = default;
        batches = default;
    }

    private readonly struct Batch
    {
        public readonly int length, kernelsCount;

        public Batch(int length, int kernelsCount)
        {
            this.length = length;
            this.kernelsCount = kernelsCount;
        }
    }

    private readonly struct JobData
    {
        public readonly T* job;
        public readonly int* degree;
        public readonly int start, end;

        public JobData(T* job, int* degree, int start, int end)
        {
            this.job = job;
            this.degree = degree;
            this.start = start;
            this.end = end;
        }
    }
}


