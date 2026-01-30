namespace TimeSliceSystem;

unsafe class TypedContainer<T> : Container where T : unmanaged, ITimeSliceJob
{
    private Bag<T> jobs = new Bag<T>(16);
    private Bag<int> frames = new Bag<int>(32);
    private Bag<Batch> batches = new Bag<Batch>(32);
    private Bag<bool> removal = new Bag<bool>(32);

    private bool disposed;

    public override void ScheduleJobs()
    {
        if (jobs.Count == 0)
            return;

        /*
         * в конце кадра пользователь может проверить какие работы были завершены 
         * в начале следующего кадра уже нельзя
         * */
        removal.RemoveRange(jobs.Count, removal.Count - jobs.Count);
        removal.Fill(false);

        for (int i = 0; i < jobs.Count; i++)
        {
            Batch batch = batches[i];
            int start = frames[i] * batch.frameStep;
            int end = Math.Min(start + batch.frameStep, batch.length);
            int round_end = end + (end % batch.kernelsCount);
            int step = (round_end - start) / batch.kernelsCount;

            if (--frames[i] < 0)
                removal[i] = true;

            T* job = jobs.GetPtr(i);
            int degree = batch.kernelsCount;
            int* deg = &degree;

            for (int j = start; j < round_end; j += step)
            {
                JobData data = new JobData(job, deg, j, Math.Min(j + step, end));
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

            while (degree > 0)
                continue;
        }

        RemoveFinishedJobs();
    }

    private void RemoveFinishedJobs()
    {
        Span<int> ids = stackalloc int[removal.Count];
        for (int i = 0; i < removal.Count; i++)
            ids[i] = i;

        int len = 0;
        for (int i = 0; i < removal.Count; i++)
        {
            if (!removal[i])
                ids[len++] = ids[i];
        }

        if (len == removal.Count)
            return;

        UpdateBag(ref jobs, ids, len);
        UpdateBag(ref frames, ids, len);
        UpdateBag(ref batches, ids, len);
    }

    private void UpdateBag<U>(ref Bag<U> bag, Span<int> ids, int length) where U : unmanaged
    {
        for (int i = 0; i < length; i++)
            bag[i] = bag[ids[i]];
        bag.RemoveRange(length, bag.Count - length);
    }

    public TimeSliceJobHandle AddJob(T job, int length, int kernelsCount, int framesCount)
    {
        framesCount = Math.Min(framesCount, length);
        int frameStep = (length + (length % framesCount)) / framesCount;
        kernelsCount = Math.Min(kernelsCount, frameStep);

        jobs.Add(job);
        batches.Add(new Batch(length, kernelsCount, frameStep));
        frames.Add(framesCount - 1);
        removal.Add(false);

        return new TimeSliceJobHandle(removal.GetPtr(removal.Count - 1));
    }

    public override void Dispose()
    {
        disposed = true;
        FreeUnmanagedResources();
    }

    ~TypedContainer()
    {
        if (!disposed)
            FreeUnmanagedResources();
    }

    private void FreeUnmanagedResources()
    {
        jobs.Dispose();
        frames.Dispose();
        batches.Dispose();
        removal.Dispose();
    }

    private readonly struct Batch
    {
        public readonly int length, kernelsCount, frameStep;

        public Batch(int length, int kernelsCount, int frameStep)
        {
            this.length = length;
            this.kernelsCount = kernelsCount;
            this.frameStep = frameStep;
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


