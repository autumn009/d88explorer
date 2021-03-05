using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrokenMOFatImageDump
{
    class Util
    {
        internal static readonly bool IsVerbose = true;
        internal static List<string> Messages = new List<string>();

        internal static string SJ2String(byte[] ar, int start, int length)
        {
            return Encoding.GetEncoding(932).GetString(ar, start, length);
        }
        static Util()
        {
            Messages.Clear();
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }
    }
}
