namespace JobSystem;

abstract class Container
{
    public Bag<int> degree;
    public Bag<Bag<JobId>> descendants;

    public void Reset()
    {
        degree = default;
        descendants = default;

        ResetInternal();
    }

    public abstract void Schedule(int jobIndex);
    protected abstract void ResetInternal();
}


