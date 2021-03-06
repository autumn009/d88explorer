using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
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
                if (item.IsDirectory)
                {
                    // ignore . and ..
                    if (item.GetFileName() == "." || item.GetFileName() == "..") continue;
                    // ignore "System Volume"
                    if (item.GetFileName() == "SYSTEM~1") continue;
                    if (Util.IsVerbose)
                    {
                        Console.WriteLine($"Sub Directory: {fullpath}");
                    }
                    var list = new List<byte[]>();
                    walkClusters(stream, dir, fat, ipl, clusterBytes, item, (buf) =>
                    {
                        list.Add(buf);
                    }, true);
                    var size = list.Select(c => c.Length).Sum();
                    var ar = new byte[size];
                    int p = 0;
                    foreach (var partArray in list)
                    {
                        Array.Copy(partArray, 0, ar, p, partArray.Length);
                        p += partArray.Length;
                    }
                    var subdir = Directory.LoadSubDir(stream, ar, ipl, fat);
                    if (subdir == null)
                    {
                        var s = $"Waring: Directory {item.GetFileName()} may borken skipped!";
                        Util.Messages.Add(s);
                        Console.WriteLine(s);
                        continue;
                    }
                    if (subdir.Entries.Length == 0) continue;
                    var newdir = Path.Combine(dstDir, item.GetFileName());
                    System.IO.Directory.CreateDirectory(newdir);
                    walkDirectory(stream, subdir, fat, ipl, newdir);
                }
                else
                {
                    if (Util.IsVerbose)
                    {
                        Console.WriteLine($"Working: {fullpath}");
                    }
                    using (var outputStream = File.Create(fullpath))
                    {
                        walkClusters(stream, dir, fat, ipl, clusterBytes, item, (buf) =>
                        {
                            outputStream.Write(buf, 0, buf.Length);
                        });
                    }
                    File.SetLastWriteTime(fullpath, item.GetDateTime());
                }
            }

            static void walkClusters(FileStream stream, Directory dir, FAT fat, IPL ipl, int clusterBytes, DirEnt item, Action<byte[]> act, bool ignoreSize = false)
            {
                var ent = item.FatEntry;
                var left = item.FileSize;
#if DEBUG
                if (ignoreSize) Console.WriteLine("Start Reading SubDir");
#endif
                for (; ; )
                {
                    if (!ignoreSize && left <= 0) break;
                    var offset = Util.OffsetFixer((ent - 2) * clusterBytes + ipl.DataAreaOffset);
                    stream.Seek(offset, SeekOrigin.Begin);
                    int s = clusterBytes;

                    //var nextFAT = fat.GetFat(ent);
                    //if (nextFAT >= 0xfff8 && nextFAT <= 0xffff)
                    //{
                    //s = (nextFAT - 0xfff8) * ipl.SectorLength;
                    //}

                    if (!ignoreSize) s = Math.Min(clusterBytes, left);
#if DEBUG
                    if (ignoreSize) System.Diagnostics.Debug.Assert(s <= 2048);
                    if (ignoreSize) Console.WriteLine($"cluster=0x{ent:X4} offset=0x{offset:X8} s={s}");
#endif
                    var buf = new byte[s];
                    stream.Read(buf, 0, s);
#if DEBUG
                    var tgt = "StartFon";
                    for (int j = 0; j < 512; j += 32)
                    {
                        for (int i = 0; i < 8; i++)
                        {
                            if (buf[i + j] != tgt[i]) goto ok;
                        }
                    }
                    if (ignoreSize) System.Diagnostics.Debug.Fail("StartFon");
                    ok:
#endif
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
