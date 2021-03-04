using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrokenMOFatImageDump
{
    class FAT
    {
        private byte[] fatall;
        internal static FAT Load(FileStream stream, IPL ipl)
        {
            var fat = new FAT();
            var fatBytes = ipl.SectorLength * ipl.TotalSectorsInOneFAT;
            fat.fatall = new byte[fatBytes];
            for (int i = 0; i < ipl.NumberOfFATs; i++)
            {
                stream.Read(fat.fatall, 0, fat.fatall.Length);
            }
            return fat;
        }
    }
}
