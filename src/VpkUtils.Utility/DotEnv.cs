namespace VpkUtils.Utility;

public class DotEnv
{
    public static void Load()
    {
        var file = Path.Combine(Directory.GetCurrentDirectory(), ".env");
        if (!File.Exists(file)) return;
        foreach (var line in File.ReadAllText(file).Split('\n'))
        {
            if (line.Length == 0 || !line.Contains('=')) continue;
            var kv = line.Split('=');
            if (kv.Length != 2) continue;
            Environment.SetEnvironmentVariable(kv[0], kv[1]);
        }
        Console.WriteLine(file);
    }
}
