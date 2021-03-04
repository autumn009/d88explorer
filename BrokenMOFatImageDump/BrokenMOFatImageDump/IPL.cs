using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrokenMOFatImageDump
{
    class IPL
    {
        internal static IPL Load(FileStream stream)
        {
            return new IPL();
        }
    }
}
