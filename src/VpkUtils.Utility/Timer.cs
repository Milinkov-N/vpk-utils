namespace VpkUtils.Utility;

public class Timer
{
    private DateTime StartTime { get; set; }
    private DateTime EndTime { get; set; }

    public void Start()
    {
        StartTime = DateTime.Now;
    }

    public void End()
    {
        EndTime = DateTime.Now;
    }

    public int ElapsedMillis()
    {
        return (EndTime - StartTime).Milliseconds;
    }
}
