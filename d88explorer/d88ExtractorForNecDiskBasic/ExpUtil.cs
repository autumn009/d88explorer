﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace d88ExtractorForNecDiskBasic
{
    static class ExpUtil
    {
        private static string badCharacters = "\\/:*?\"<>|";
        private static string[] badNames = {"con", "aux","prn","nul",
            "com0",
            "com1",
            "com2",
            "com3",
            "com4",
            "com5",
            "com6",
            "com7",
            "com8",
            "com9",
            "lpt0",
            "lpt1",
            "lpt2",
            "lpt3",
            "lpt4",
            "lpt5",
            "lpt6",
            "lpt7",
            "lpt8",
            "lpt9",
        };

        internal static string ConvertD88FileNameToHostFileName(string filename)
        {
            var body = filename.Substring(0, 6).Trim();
            var bodyLower = body.ToLower();
            if (badNames.Any(c => c == bodyLower)) body = body + "_";
            var ext = filename.Substring(6).Trim();
            var sb = new StringBuilder();
            foreach (var item in body + "." + ext)
            {
                if (badCharacters.Contains(item))
                    sb.AppendFormat("%{0:X2}", (int)item);
                else
                    sb.Append(item);
            }
            return sb.ToString();
        }
    }
}
