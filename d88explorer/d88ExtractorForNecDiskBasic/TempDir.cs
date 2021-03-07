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

        internal FileDetails[] CreateImage(VDisk vdisk)
        {
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
