using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrokenMOFatImageDump
{
    class IPL
    {
        private byte[] sector;
        public int SectorLength;
        public int ClusterPerSector;
        public int FATStartSector;
        public int NumberOfFATs;
        public int TotalRootDirectories;
        public int TotalLogocalSectors;
        public byte MediaDescripter;
        public int TotalSectorsInOneFAT;

        internal static IPL Load(FileStream stream)
        {
            var ipl = new IPL();
            ipl.sector = new byte[512];
            stream.Read(ipl.sector, 0, ipl.sector.Length);
            if (ipl.sector[0] != 0xeb) throw new ApplicationException("ipl.sector[0] must be 0xeb");
            if (ipl.sector[1] != 0x3c) throw new ApplicationException("ipl.sector[1] must be 0x3c");
            if (ipl.sector[2] != 0x90) throw new ApplicationException("ipl.sector[2] must be 0x90");
            ipl.SectorLength = ipl.sector[0xb] + ipl.sector[0xc] << 8;

            if (Util.IsVerbose)
            {
                Console.WriteLine($"SectorLength={ipl.SectorLength}");
            }

            return ipl;

        }
    }
}
