using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BrokenMOFatImageDump
{
    class FatImage
    {
        private static void walkDirectory(FileStream stream, Directory dir, FAT fat, IPL ipl, string dstDir)
        {
            int clusterBytes = ipl.SectorLength * ipl.ClusterPerSector;
            foreach (var item in dir.Entries)
            {
                var fullpath = Path.Combine(dstDir, item.GetFileName());
                if (item.IsVolumeLabel)
                {
                    if (Util.IsVerbose)
                    {
                        Console.WriteLine($"Volume Label Detected: {fullpath}");
                    }
                }
                if (Util.IsVerbose)
                {
                    Console.WriteLine($"Working: {fullpath}");
                }
                if (item.IsDirectory)
                {
                    // TBW
                }
                else
                {
                    using var outputStream = File.Create(fullpath);
                    walkCusters(stream, dir, fat, clusterBytes, item, outputStream, (buf)=> {
                        outputStream.Write(buf, 0, buf.Length);
                    });
                }
            }

            static void walkCusters(FileStream stream, Directory dir, FAT fat, int clusterBytes, DirEnt item, FileStream outputStream, Action<byte[]> act)
            {
                var ent = item.FatEntry;
                var left = item.FileSize;
                for (; ; )
                {
                    if (left <= 0) break;
                    stream.Seek((ent - 2) * clusterBytes + dir.DataAreaOffset, SeekOrigin.Begin);
                    var s = Math.Min(clusterBytes, left);
                    var buf = new byte[s];
                    stream.Read(buf, 0, s);
                    act(buf);
                    left -= s;
                    ent = fat.GetFat(ent);
                    // 必要ないはずであるが念のために入れてある
                    // 無効クラスタ番号が出てきたら終了
                    if (ent < 2 || ent > 0xffef) break;
                }
            }
        }
        internal static void Dump(string filename)
        {
            var dstDir = Path.Combine(Path.GetDirectoryName(filename),Path.GetFileNameWithoutExtension(filename));
            System.IO.Directory.CreateDirectory(dstDir);
            using var stream = File.OpenRead(filename);
            var ipl = IPL.Load(stream);
            var fat = FAT.Load(stream, ipl);
            var root = Directory.LoadRoot(stream, ipl, fat);
            walkDirectory(stream, root, fat, ipl, dstDir);
            Console.WriteLine("Done");
        }
    }
}
