using d88lib;
using System;
using System.IO;

namespace d88ExtractorForNecDiskBasic
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("usage: d88ExtractorForNecDiskBasic SRC_FILES_WITH_WILDCARD");
                return;
            }
            foreach (var pathWithWildCard in args)
            {
                if (pathWithWildCard == "-9") continue;
                foreach (var fullpath in Directory.GetFiles(Path.GetDirectoryName(pathWithWildCard), Path.GetFileName(pathWithWildCard)))
                {
                    VDisk currentVDisk = new VDisk(File.ReadAllBytes(fullpath)); ;
                    var outputDirectoryName = Path.ChangeExtension(fullpath, null);
                    TempDir currentTempDir = new TempDir(outputDirectoryName);
                    FileDetails[] currentImage = currentTempDir.CreateImage(currentVDisk);


                }
            }





        }
    }
}
