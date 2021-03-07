using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace BrokenMOFatImageDump
{
    class FAT
    {
        private byte[] fatall;
        internal int GetFat(int index)
        {
            var offset = index * 2;
            return fatall[offset] + (fatall[offset + 1] << 8);
        }

        private void fat12Fixer()
        {
            byte[] nibbleBuf = new byte[fatall.Length * 2];
            int p = 0;
            for (int i = 0; i < fatall.Length; i++)
            {
                nibbleBuf[p++] = (byte)(fatall[i] & 0xf);
                nibbleBuf[p++] = (byte)(fatall[i] >> 4);
            }
            byte[] fat12Buf = new byte[nibbleBuf.Length / 3 + 2];
            int np = 0;
            for (int i = 0; i < fat12Buf.Length / 2; i++)
            {
                int n = nibbleBuf[np++];
                n |= nibbleBuf[np++] << 4;
                n |= nibbleBuf[np++] << 8;
                if (n >= 0xff7) n |= 0xf000;
                fat12Buf[i * 2] = (byte)(n & 0xff);
                fat12Buf[i * 2 + 1] = (byte)(n >> 8);
            }
            // swap to created Psudo FAT16 data
            fatall = fat12Buf;
        }

        internal static FAT Load(FileStream stream, IPL ipl)
        {

            var fat = new FAT();
            var fatBytes = ipl.SectorLength * ipl.TotalSectorsInOneFAT;
            fat.fatall = new byte[fatBytes];
            for (int i = 0; i < ipl.NumberOfFATs; i++)
            {
                stream.Read(fat.fatall, 0, fat.fatall.Length);
            }
            if (ipl.FormatType == EFormatType.FAT12) fat.fat12Fixer();
            return fat;
        }
    }
}
