namespace JobSystem;

public static class JobManager
{
    private static Dictionary<nint, Container> containers = new(64);
    private static List<JobId> stack = new(64);

    public static void Run()
    {
        foreach (JobId id in stack)
            containers[id.type].Schedule(id.index);

        while (stack.Count > 0)
        {
            int i = 0;
            while (i < stack.Count)
            {
                JobId id = stack[i];
                Container container = containers[id.type];

                if (container.degree[id.index] != 0)
                {
                    i++;
                    continue;
                }

                stack.RemoveAt(i);

                Bag<JobId> descendants = container.descendants[id.index];
                for (int j = 0; j < descendants.Count; j++)
                {
                    JobId des_id = descendants[j];
                    Container des_cont = containers[des_id.type];
                    if (--des_cont.degree[des_id.index] == 0)
                    {
                        des_cont.Schedule(des_id.index);
                        stack.Add(des_id);
                    }
                }
            }
        }

        JobSysServ.Arena.Reset();
        foreach (Container container in containers.Values)
            container.Reset();
    }

    public static JobHandle AddJob<T>(T job, JobHandle handle) where T : unmanaged, IJob
    {
        JobContainer<T> container;
        if (containers.TryGetValue(typeof(T).TypeHandle.Value, out Container? cont))
            container = (JobContainer<T>)cont;
        else
            containers.Add(typeof(T).TypeHandle.Value, container = new JobContainer<T>());

        JobHandle result = container.AddJob(job, handle);
        if (handle.Length == 0)
        {
            stack.Add(result[0]);
            return result;
        }

        for (int i = 0; i < handle.Length; i++)
        {
            JobId id = handle[i];
            containers[id.type].descendants[id.index].Add(result[0]);
        }

        return result;
    }

    public static JobHandle AddJobParallel<T>(T job, int length, int kernelCount, JobHandle handle) where T : unmanaged, IJobParallel
    {
        JobParallelContainer<T> container;
        if (containers.TryGetValue(typeof(T).TypeHandle.Value, out Container? cont))
            container = (JobParallelContainer<T>)cont;
        else
            containers.Add(typeof(T).TypeHandle.Value, container = new JobParallelContainer<T>());

        JobHandle result = container.AddJob(job, length, kernelCount, handle);
        if (handle.Length == 0)
        {
            stack.Add(result[0]);
            return result;
        }

        for (int i = 0; i < handle.Length; i++)
        {
            JobId id = handle[i];
            containers[id.type].descendants[id.index].Add(result[0]);
        }

        return result;
    }
}


