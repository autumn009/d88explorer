﻿using System;
using System.IO;
using System.Linq;

namespace d88toSimpleImage
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("usage: d88toSimpleImage [-9] SRC_FILES_WITH_WILDCARD");
                Console.WriteLine("Convert *.d88 to *.disk (DD Disk Image)");
                Console.WriteLine("-9: remove sector 9 with 0xe5");
                return;
            }
            bool isRemove9 = args.Any(c => c == "-9");
            foreach (var pathWithWildCard in args)
            {
                if (pathWithWildCard == "-9") continue;
                foreach (var fullpath in Directory.GetFiles(Path.GetDirectoryName(pathWithWildCard), Path.GetFileName(pathWithWildCard)))
                {
                    convertIt(fullpath);
                }
            }

            void convertIt(string fullpath)
            {
                var dstpath = Path.ChangeExtension(fullpath, ".disk");
                Console.WriteLine($"Converting {fullpath} to {dstpath}");
                var srcImage = File.ReadAllBytes(fullpath);
                var tracks = srcImage[17 + 9 + 1] switch
                {
                    0 => 40,
                    0x10 => 80,
                    0x20 => 77,
                    _ => 0,
                };
                if (tracks == 0) throw new ApplicationException($"disk type {srcImage[17 + 9 + 1]} is invalid");
                // double sided
                tracks *= 2;
                using var outStream = File.Create(dstpath);
                var offset = 17 + 9 + 1 + 1 + 4;
                for (int track = 0; track < tracks; track++)
                {
                    Console.Write($"Track: {track} ");
                    int sectorOffset = getOffset(srcImage, offset);
                    int endOffset = srcImage.Length;
                    if (track < tracks - 1)
                    {
                        endOffset = getOffset(srcImage, offset + 4);
                    }
                    var sectors = 0;
                    for (; ; )
                    {
                        if (sectorOffset >= endOffset) break;
                        var size = srcImage[sectorOffset + 3] switch
                        {
                            0 => 128,
                            1 => 256,
                            2 => 512,
                            3 => 1024,
                            _ => 0,
                        };
                        if (size == 0) throw new ApplicationException($"size {srcImage[sectorOffset + 3]} is invalid");
                        sectorOffset += 1 + 1 + 1 + 1 + 2 + 1 + 1 + 1 + 5 + 2;
                        //System.Diagnostics.Debug.Assert(sectorOffset + size < srcImage.Length);
                        //if (sectorOffset + size >= srcImage.Length) break;
                        bool writeFlag = true;
                        if (isRemove9 && sectors == 8)
                        {
                            for (int i = 0; i < size; i++)
                            {
                                if (srcImage[sectorOffset + i] != 0xe5)
                                {
                                    throw new ApplicationException($"Sector 9 has not 0xe5 in track {track}");
                                }
                            }
                            writeFlag = false;
                        }
                        if(writeFlag) outStream.Write(srcImage, sectorOffset, size);
                        sectorOffset += size;
                        sectors++;
                    }
                    offset += 4;
                    Console.WriteLine($"{sectors} Sectors Written");
                }
            

                static int getOffset(byte[] srcImage, int offset)
                {
                    int sectorOffset = srcImage[offset];
                    sectorOffset |= srcImage[offset + 1] << 8;
                    sectorOffset |= srcImage[offset + 2] << 16;
                    sectorOffset |= srcImage[offset + 3] << 24;
                    return sectorOffset;
                }
            }
        }
    }
}
