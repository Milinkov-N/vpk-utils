namespace VpkUtils.Clap;

public class UnexpectedArgumentException : Exception
{
    public UnexpectedArgumentException(string? arg)
        : base($"unexpected argument '{arg}' found")
    {
        
    }

    public UnexpectedArgumentException(string? arg, Exception? inner)
    : base($"unexpected argument '{arg}' found", inner)
    {

    }
}
