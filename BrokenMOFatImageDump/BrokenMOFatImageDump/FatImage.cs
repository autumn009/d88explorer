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
        internal static void Dump(string filename)
        {
            var stream = File.OpenRead(filename);
            var ipl = IPL.Load(stream);
            var fat = FAT.Load(stream, ipl);
            var root = Directory.LoadRoot(stream, ipl, fat);

            Console.WriteLine("Done");
        }
    }
}
