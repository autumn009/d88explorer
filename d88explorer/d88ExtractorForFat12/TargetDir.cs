using d88lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace d88ExtractorForFat12
{
    internal struct SectorInfo
    {
        public int Sector { get;set; }
        public int Track { get; set; }
        public int Surface { get; set; }
    }

    internal class TargetDir
    {
        public string OutputDirectoryName { get; }
        public TargetDir(string outputDirectoryName)
        {
            OutputDirectoryName = outputDirectoryName;
        }
        internal SectorInfo CalcSectorInfo(VDisk vDisk, int clusterNumber)
        {




        }



        internal byte[] GetCluster(VDisk vDisk)
        {
            vDisk.

        }

        internal void CreateImage(VDisk vDisk)
        {



        }
    }
}
