
namespace VpkUtils.App;

using Utility;

internal class FileRenamer(FileRenamerOptions opt)
{
    public FileRenamerOptions GetOptions()
    {
        return opt;
    }

    public void Run(bool dryRun)
    {
        var allFiles = Directory.EnumerateFiles(opt.Path);
        var filteredFiles = opt.FilterByExt is not null
            ? allFiles.Where(d => Path.GetExtension(d) == opt.FilterByExt)
            : allFiles;
        foreach (var entry in filteredFiles)
        {
            var oldName = Path.GetFileName(entry);
            var newName = ConstructNewFilename(
                Utility.ExtractIndexFromFileName(oldName),
                opt.NewName,
                opt.NewNameSuffix ?? "",
                Path.GetExtension(entry)
            );

            if (!dryRun)
            {
               if (!File.Exists(newName)) File.Move(entry, Path.Combine(opt.Path, newName));
            }

            if (opt.LogFunc is not null) opt.LogFunc(entry, newName);
        }
    }

    private static string ConstructNewFilename(
        int idx,
        string newName,
        string suffix,
        string ext
    )
    {
        if (!ext.StartsWith('.'))
            throw new ArgumentException("file extension should start with '.' character");
        return $"{newName}_{idx:d2}{suffix}{ext}";
    }
}

internal class FileRenamerOptions
{
    public required string Path { get; set; }
    public required string NewName { get; set; }
    public string? FilterByExt { get; set; }
    public string? NewNameSuffix { get; set; }

    public Action<string, string>? LogFunc { get; set; }
}
