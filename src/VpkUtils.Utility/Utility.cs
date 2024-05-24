namespace VpkUtils.Utility;

public static class Utility
{
    public static int ExtractIndexFromFileName(string fileName)
    {
        var space = fileName.IndexOf(' ');
        var sep = space == -1 ? fileName.IndexOf('.') : space;
        return int.Parse(fileName[..sep]);
    }
}
