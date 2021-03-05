using System;

namespace BrokenMOFatImageDump
{
    class Program
    {
        static void Main(string[] args)
        {
            if( args.Length != 1)
            {
                Console.WriteLine("usage: BrokenMOFatImageDump srcImage");
                return;
            }
            FatImage.Dump(args[0]);
            foreach (var item in Util.Messages)
            {
                Console.WriteLine(item);
            }
        }
    }
}
