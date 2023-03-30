using d88lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace d88ExtractorForFat12
{
    internal class TargetDir
    {
        public TargetDir(string outputDirectoryName)
        {
            OutputDirectoryName = outputDirectoryName;
        }

        public string OutputDirectoryName { get; }

        internal void CreateImage(VDisk currentVDisk)
        {
            throw new NotImplementedException();
        }
    }
}
