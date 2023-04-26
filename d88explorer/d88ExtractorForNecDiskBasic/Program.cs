using d88lib;
using System;
using System.Collections.Generic;
using System.IO;

namespace d88ExtractorForNecDiskBasic
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("usage: d88ExtractorForNecDiskBasic SRC_FILES_WITH_WILDCARD [-d DST_DIR]");
                return;
            }
            var list = new List<string>();
            string dst = null;
            bool dFlag = false;
            foreach (var pathWithWildCard in args)
            {
                if (dFlag)
                {
                    dst = pathWithWildCard;
                    dFlag = false;
                    continue;
                }
                if (pathWithWildCard == "-9") continue;
                if (pathWithWildCard == "-d") dFlag = true;
                else list.Add(pathWithWildCard);
            }
            foreach (var pathWithWildCard in list)
            {
                foreach (var fullpath in Directory.GetFiles(Path.GetDirectoryName(pathWithWildCard), Path.GetFileName(pathWithWildCard)))
                {
                    VDisk currentVDisk = new VDisk(File.ReadAllBytes(fullpath));
                    var outputDirectoryName = Path.Combine(dst,Path.GetFileNameWithoutExtension(fullpath));
                    if (dst == null) outputDirectoryName = Path.ChangeExtension(fullpath, null);
                    TempDir currentTempDir = new TempDir(outputDirectoryName);
                    FileDetails[] dummyCurrentImage = currentTempDir.CreateImage(currentVDisk);
                }
            }
        }
    }
}
