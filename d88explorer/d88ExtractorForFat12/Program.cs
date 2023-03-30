using d88ExtractorForFat12;
using d88lib;

if (args.Length == 0)
{
    Console.WriteLine("usage: d88ExtractorForFat12 SRC_FILES_WITH_WILDCARD");
    return;
}
foreach (var pathWithWildCard in args)
{
    foreach (var fullpath in Directory.GetFiles(Path.GetDirectoryName(pathWithWildCard??""), Path.GetFileName(pathWithWildCard)))
    {
        VDisk currentVDisk = new VDisk(File.ReadAllBytes(fullpath)); ;
        var outputDirectoryName = Path.ChangeExtension(fullpath, null);
        TargetDir currentTempDir = new TargetDir(outputDirectoryName);
        currentTempDir.CreateImage(currentVDisk);
    }
}
