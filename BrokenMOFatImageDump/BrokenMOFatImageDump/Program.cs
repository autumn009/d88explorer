using System;
using System.Collections.Generic;

namespace BrokenMOFatImageDump
{
    class Program
    {
        static void Main(string[] args)
        {
            if( args.Length == 0)
            {
                Console.WriteLine("usage: [-f] BrokenMOFatImageDump srcImage");
                Console.WriteLine("-f for fix blocken MO image");
                return;
            }
            var list = new List<string>();
            foreach (var item in args)
            {
                if (item == "-f")
                    Util.IsFixBrokenMO = true;
                else
                    list.Add(item);
            }
            foreach (var item in list)
            {
                FatImage.Dump(item);
            }
            foreach (var item in Util.Messages)
            {
                Console.WriteLine(item);
            }
        }
    }
}
