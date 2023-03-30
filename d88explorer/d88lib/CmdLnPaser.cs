using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace d88lib
{
    public static class CmdLnPaser
    {
        public static bool Parse()
        {
            foreach (var item in Environment.GetCommandLineArgs().Skip(1))
            {
                if( item.StartsWith("-"))
                {
                    ErrorMessage = $"{item} is not a valid option";
                    return false;
                }
                FileName = item;
            }
            return true;
        }

        public static string FileName { get; private set; }

        public static string ErrorMessage { get; private set; }
    }
}
