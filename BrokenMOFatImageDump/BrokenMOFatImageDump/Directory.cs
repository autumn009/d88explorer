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
            for (int i = 0; i < size; i+= DirEnt.DirEntSize)
            {
                if (all[i] == 0) break;
                if (all[i] == 0xe5) continue;
                for (int j = 0; j < 11; j++)
                {
                    var c = all[i + j];
                    if (c != 0) Console.Write($"{(char)c}");
                }
                Console.WriteLine();
            }
            dir.Entries = list.ToArray();
            return dir;
        }
    }
}
