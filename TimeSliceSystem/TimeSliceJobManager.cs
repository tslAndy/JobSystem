namespace TimeSliceSystem;

// TODO delete idle containers
public static class TimeSliceJobManager
{
    private static Dictionary<IntPtr, Container> containers = new();

    private static IntPtr[]? update_order;
    private static List<IntPtr> sorted_keys = new(8);
    private static bool update_keys;


    public static void SetUpdateOrder(Type[] types)
    {
        update_order = new IntPtr[types.Length];
        for (int i = 0; i < types.Length; i++)
            update_order[i] = types[i].TypeHandle.Value;
    }

    public static void RunJobs()
    {
        if (update_keys)
        {
            update_keys = false;
            if (update_order != null)

                sorted_keys.Sort((x, y) => Array.IndexOf(update_order, x) - Array.IndexOf(update_order, y));
        }

        foreach (IntPtr key in sorted_keys)
            containers[key].ScheduleJobs();
    }

    public static TimeSliceJobHandle AddJob<T>(T job, int length, int kernelsCount, int framesCount) where T : unmanaged, ITimeSliceJob
    {
        TypedContainer<T> container;
        if (containers.TryGetValue(typeof(T).TypeHandle.Value, out Container? cont))
        {
            container = (TypedContainer<T>)cont;
        }
        else
        {
            containers.Add(typeof(T).TypeHandle.Value, container = new TypedContainer<T>());
            sorted_keys.Add(typeof(T).TypeHandle.Value);
            update_keys = true;
        }
        return container.AddJob(job, length, kernelsCount, framesCount);
    }
}


