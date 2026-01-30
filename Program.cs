using JobSystem;

MyJob job_1 = new();
MyJob job_2 = new();

JobHandle handle_1 = job_1.Schedule() + job_2.Schedule();
JobManager.Run();

struct MyJob : IJob
{
    public void Execute()
    {
        Console.WriteLine("hello");
    }
}
