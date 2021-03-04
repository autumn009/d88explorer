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
            ipl.SectorLength = ipl.sector[0xb] + (ipl.sector[0xc] << 8);
            ipl.ClusterPerSector = ipl.sector[0xd];
            ipl.FATStartSector = ipl.sector[0xe] + (ipl.sector[0xf] << 8);
            ipl.NumberOfFATs = ipl.sector[0x10];
            ipl.TotalRootDirectories = ipl.sector[0x11] + (ipl.sector[0x12] << 8);
            ipl.TotalLogocalSectors = ipl.sector[0x13] + (ipl.sector[0x14] << 8);
            ipl.MediaDescripter = ipl.sector[0x15];
            ipl.TotalSectorsInOneFAT = ipl.sector[016] + (ipl.sector[0x17] << 8);

            if (Util.IsVerbose)
            {
                Console.WriteLine($"SectorLength={ipl.SectorLength}");
                Console.WriteLine($"ClusterPerSector={ipl.ClusterPerSector}");
                Console.WriteLine($"FATStartSector={ipl.FATStartSector}");
                Console.WriteLine($"NumberOfFATs={ipl.NumberOfFATs}");
                Console.WriteLine($"TotalRootDirectories={ipl.TotalRootDirectories}");
                Console.WriteLine($"TotalLogocalSectors={ipl.TotalLogocalSectors}");
                Console.WriteLine($"MediaDescripter=0x{ipl.MediaDescripter:X2}");
                Console.WriteLine($"TotalSectorsInOneFAT={ipl.TotalSectorsInOneFAT}");
            }

            return ipl;

        }
    }
}
