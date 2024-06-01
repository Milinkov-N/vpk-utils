namespace VpkUtils.Utility;

public class Logger
{
    public static void Log(string entry, string newName)
    {
        Console.WriteLine($"\t{Path.GetFileName(entry)}\t-->\t{newName}");
    }
}
