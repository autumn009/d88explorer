using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrokenMOFatImageDump
{
    class DirEnt
    {
        internal const int DirEntSize = 32;
        internal string FileName;
        internal string FileExt;
        internal byte Attributes;
        internal int Time;
        internal int Date;
        internal int FatEntry;
        internal int FileSize;

        internal string GetFileName()
        {
            if( string.IsNullOrWhiteSpace(FileExt))
            {
                return FileName;
            }
            return $"{FileName}.{FileExt}";
        }

        internal void Dump()
        {
            Console.WriteLine(GetFileName());
        }
    }

    class Directory
    {

        internal DirEnt[] Entries;
        internal static Directory LoadRoot(FileStream stream, IPL ipl, FAT fat)
        {
            var dir = new Directory();
            var list = new List<DirEnt>();
            var size = ipl.TotalRootDirectories * DirEnt.DirEntSize;
            byte[] all = new byte[size];
            stream.Read(all, 0, all.Length);
            for (int i = 0; i < size; i += DirEnt.DirEntSize)
            {
                if (all[i] == 0) break;
                if (all[i] == 0xe5) continue;
                var ent = new DirEnt();
                string fn = "";
                for (int j = 0; j < 8; j++)
                {
                    var c = all[i + j];
                    if (c != 0) fn += ((char)c).ToString();
                }
                ent.FileName = fn.Trim();
                string fx = "";
                for (int j = 0; j < 3; j++)
                {
                    var c = all[i + 8 + j];
                    if (c != 0) fx += ((char)c).ToString();
                }
                ent.FileExt = fx.Trim();
                if (Util.IsVerbose) ent.Dump();
            }
            dir.Entries = list.ToArray();
            return dir;
        }
    }
}
