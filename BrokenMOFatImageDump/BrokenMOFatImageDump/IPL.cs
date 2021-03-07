using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrokenMOFatImageDump
{
    enum EFormatType { FAT12, FAT16 }
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
        public EFormatType FormatType;

        internal int DataAreaOffset;

        private void DetectFormatType()
        {
            const string fat12 = "FAT12   ";
            const string fat16 = "FAT16   ";

            if (sector[0] != 0xeb) throw new ApplicationException("ipl.sector[0] must be 0xeb");
            if (sector[2] != 0x90) throw new ApplicationException("ipl.sector[2] must be 0x90");
            switch (sector[1])
            {
                case 0x1c:
                    FormatType = EFormatType.FAT12;
                    return;
                case 0x3c:
                    break;
                default:
                    throw new ApplicationException("ipl.sector[1] must be 0x1c or 0x3c");
            }
            var sb = new StringBuilder();
            for (int i = 0; i < 8; i++) sb.Append((char)sector[0x36 + i]);
            var s = sb.ToString();
            switch (s)
            {
                case fat12:
                    FormatType = EFormatType.FAT12;
                    break;
                case fat16:
                    FormatType = EFormatType.FAT16;
                    break;
                default:
                    throw new ApplicationException($"Format {s} Not Supported");
            }
        }

        internal static IPL Load(FileStream stream)
        {
            var ipl = new IPL();
            ipl.sector = new byte[512];
            stream.Read(ipl.sector, 0, ipl.sector.Length);
            ipl.DetectFormatType();
            ipl.SectorLength = ipl.sector[0xb] + (ipl.sector[0xc] << 8);
            ipl.ClusterPerSector = ipl.sector[0xd];
            ipl.FATStartSector = ipl.sector[0xe] + (ipl.sector[0xf] << 8);
            ipl.NumberOfFATs = ipl.sector[0x10];
            ipl.TotalRootDirectories = ipl.sector[0x11] + (ipl.sector[0x12] << 8);
            ipl.TotalLogocalSectors = ipl.sector[0x13] + (ipl.sector[0x14] << 8);
            ipl.MediaDescripter = ipl.sector[0x15];
            ipl.TotalSectorsInOneFAT = ipl.sector[0x16] + (ipl.sector[0x17] << 8);

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
                Console.WriteLine($"FormatType={ipl.FormatType}");
            }

            return ipl;

        }
    }
}
