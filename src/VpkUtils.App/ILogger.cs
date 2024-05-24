namespace VpkUtils.App;

internal interface ILogger
{
    void Log(string dirEntry, string newName);
}

internal class ConsoleLogger : ILogger
{
    public void Log(string dirEntry, string newName)
    {
        Console.WriteLine($"\t{Path.GetFileName(dirEntry)}\t-->\t{newName}");
    }
}
