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
            foreach (var item in dir.Entries)
            {
                var fullpath = Path.Combine(dstDir, item.GetFileName());
                if (Util.IsVerbose)
                {
                    Console.WriteLine($"Working: {fullpath}");
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
