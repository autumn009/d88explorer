using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using d88lib;
using System.Threading;

namespace d88ExtractorForNecDiskBasic
{
    class FileDetails
    {
        internal DirectoryEntry OriginalEntry { get; set; }
        internal string HostFileSystemFullPath { get; set; }
    }

    class TempDir
    {
        private string tempFolderName;

        private bool validateFat(VDisk vdisk)
        {
            byte[] fat = vdisk.GetFat();
            for (int i = 0; i < fat.Length; i++)
            {
                if (fat[i] == 0xff) continue;
                if (fat[i] == 0xfe) continue;
                if (fat[i] >= 0xc1) continue;
                if (fat[i] < fat.Length)
                {
                    // chain checker
                    var j = fat[i];
                    for (; ; )
                    {
                        if (fat[j] == 0xff) return false;
                        if (fat[j] == 0xfe) return false;
                        if (fat[j] >= 0xc1) break;
                        if (fat[j] >= fat.Length) return false;
                        j = fat[j];
                    }
                }
                else return false;
            }
            return true;
        }

        internal FileDetails[] CreateImage(VDisk vdisk, string src)
        {
            if(!validateFat(vdisk))
            {
                Console.WriteLine($"{src} may not a NEC format, Skipped");
                return null;
            }

            Directory.CreateDirectory(tempFolderName);
            var details = new List<FileDetails>();
            foreach (var item in vdisk.EnumFiles())
            {
                var cookedFileName = ExpUtil.ConvertD88FileNameToHostFileName(item.FileName);
                var fullpath = Path.Combine(tempFolderName, cookedFileName);
                details.Add(new FileDetails() { OriginalEntry = item, HostFileSystemFullPath = fullpath });
            }
            // check duplication
            for (; ; )
            {
                bool ok = true;
                foreach (var item in details)
                {
                    var tgt = item.HostFileSystemFullPath.ToLower();
                    if (details.Count(c => c.HostFileSystemFullPath.ToLower() == tgt) == 1) continue;
                    int order = 0;
                    foreach (var item2 in details)
                    {
                        if (tgt == item2.HostFileSystemFullPath.ToLower())
                        {
                            // TBW add number to body, not ext
                            item2.HostFileSystemFullPath += order++;
                            ok = false;
                        }
                    }
                }
                if (ok) break;
            }
            // create image
            foreach (var item in details)
            {
                using (var outputStrem = File.Create(item.HostFileSystemFullPath))
                {
                    vdisk.EnumFileClusters(item.OriginalEntry.FirstFATNumber, (image, offset, len) =>
                     {
                         outputStrem.Write(image, offset, len);
                     });
                }
            }
            return details.ToArray();
        }

        internal void ShowTempFolder()
        {
            System.Diagnostics.Process.Start("explorer.exe", tempFolderName);
        }

        public TempDir(string outDir)
        {
            tempFolderName = outDir;
        }
    }
}
