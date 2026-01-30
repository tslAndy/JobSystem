namespace TimeSliceSystem;

public unsafe readonly struct TimeSliceJobHandle
{
    private readonly bool* finished;

    public TimeSliceJobHandle(bool* finished) => this.finished = finished;

    public bool IsFinished => *finished;
}


