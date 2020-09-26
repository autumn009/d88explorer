using System;
using System.Collections.Generic;
using System.Text;

namespace d88lib
{
    public enum ETypeOfDisk
    {
        Type2D = 0,
        Type2DD = 0x10,
        Type2HD = 0x20
    }
    public enum EDensity
    {
        DoubleDensoty = 0,
        HighDensoty = 1,
        SingleDensoty = 0x40
    }
    public class DirectoryEntry
    {
        public readonly int Id;
        public readonly string FileName;
        public readonly bool IsBinaryFile;
        public readonly bool IsReadAfterWriteFile;
        public readonly bool IsProtectedFile;
        public readonly bool IsMachineLanguageFile;
        public readonly bool IsWriteProtectFile;
        public readonly int FirstFATNumber;

        public DirectoryEntry(int id, byte[] diskImage, int offset)
        {
            Id = id;
            FileName = Encoding.GetEncoding("ISO-8859-1").GetString(diskImage, offset, 9);
            FirstFATNumber = diskImage[offset + 10];
            IsMachineLanguageFile = (diskImage[offset + 9] & 1) != 0;
            IsWriteProtectFile = (diskImage[offset + 9] & 0x10) != 0;
            IsProtectedFile = (diskImage[offset + 9] & 0x20) != 0;
            IsReadAfterWriteFile = (diskImage[offset + 9] & 0x40) != 0;
            IsBinaryFile = (diskImage[offset + 9] & 0x80) != 0;
        }
    }
    public class TrackInfo
    {
        public readonly int Surface;
        public readonly int Sector;
        public readonly int BytesInSectorN;
        public readonly int SectorsInTrack;
        public readonly EDensity Density;
        public readonly bool IsDeletedData;
        public readonly int Status;
        public readonly int Length;
        public TrackInfo(byte[] diskImage, int offset)
        {
            Surface = diskImage[offset + 0];
            Sector = diskImage[offset + 1];
            BytesInSectorN = diskImage[offset + 3];
            SectorsInTrack = diskImage[offset + 4] + diskImage[offset + 5] * 256;
            Density = (EDensity)(diskImage[offset + 6]);
            IsDeletedData = diskImage[offset + 7] == 0x10;
            Status = diskImage[offset + 8];
            Length = diskImage[offset + 14] + diskImage[offset + 15] * 256;
        }
    }

    public class VDisk
    {
        private const int BytesPerSector = 256;
        private byte[] diskImage;

        public string ImageName
        {
            get
            {
                var list = new List<byte>();
                for (int i = 0; i < 16; i++)
                {
                    if (diskImage[i] == 0) break;
                    list.Add(diskImage[i]);
                }
                // Is it UTF8?
                return Encoding.UTF8.GetString(list.ToArray());
            }
        }

        private void ConvertClusterToTrackSurfaceSector(int cluster, out int track, out int surface, out int sector)
        {
            int clusterInTrack = sectorsInTrack / sectorsInCluster;
            track = cluster / clusterInTrack / MaxSurfaces;
            surface = (cluster / clusterInTrack) % MaxSurfaces;
            sector = (cluster % clusterInTrack) * sectorsInCluster;
        }

        private int GetOffsetFromCluster(int cluster)
        {
            ConvertClusterToTrackSurfaceSector(cluster, out int track, out int surface, out int sector);
            GetSectorDataOffsetAndLength(track, surface, sector, out int offset, out _);
            return offset;
        }

        public void EnumFileClusters(int firstFATNumber, Action<byte[], int, int> writer)
        {
            var fat = getFat();
            int cluster = firstFATNumber;
            for (; ; )
            {
                bool done = false;
                //int offset = cluster * BytesPerSector * sectorsInCluster;
                var offset = GetOffsetFromCluster(cluster);
                cluster = fat[cluster];
                int sectors = sectorsInCluster;
                if (cluster >= 0xc0)
                {
                    sectors = cluster - 0xc0;
                    done = true;
                }
                writer(diskImage, offset, BytesPerSector * sectors);
                if (done) break;
            }
        }

        public bool IsWriteProtected => diskImage[0x1c] != 0;
        public ETypeOfDisk TypeOfDisk => (ETypeOfDisk)diskImage[0x1d];
        public uint SizeOfDisk => diskImage[0x1e] + diskImage[0x1e] * 256u + diskImage[0x20] * 65536u + diskImage[0x21] * 65536u * 256u;
        public readonly int AvailableTrack;
        public readonly int MaxSurfaces;
        private readonly int sectorSizeWithMiscHeader;
        private readonly int sectorsInTrack;
        private readonly int sectorsInCluster;

        public int GetTrackOffset(int track)
        {
#if true
            var bytesInTrack = sectorSizeWithMiscHeader * sectorsInTrack;
            return 0x2b0 + track * bytesInTrack;
#else
            int offset = 0x20 + track * 4;
            uint result = diskImage[offset+0]
                + diskImage[offset + 1] * 256u
                + diskImage[offset + 2] * 65536u
                + diskImage[offset + 3] * 65536u * 256u;
            return (int)result;
#endif
        }

        private int calcOffset(int trackNumber, int surfaceNumber, int sectorNumber)
        {
            int offset = GetTrackOffset(trackNumber * MaxSurfaces + surfaceNumber)
             + sectorNumber * sectorSizeWithMiscHeader;
            return offset;
        }

        public TrackInfo GetSectorInfo(int trackNumber, int surfaceNumber, int sectorNumber)
        {
            int offset = calcOffset(trackNumber, surfaceNumber, sectorNumber);
            return new TrackInfo(diskImage, offset);
        }

        public void GetSectorDataOffsetAndLength(int trackNumber, int surfaceNumber, int sectorNumber,
            out int offset, out int length)
        {
            offset = calcOffset(trackNumber, surfaceNumber, sectorNumber);
            length = diskImage[offset + 14] + diskImage[offset + 15] * 256;
            offset += 16;
        }

        private void getIdSector(out int track, out int surface, out int sector)
        {
            switch (AvailableTrack)
            {
                case 35:
                case 40:
                    track = 18;
                    surface = 0;
                    sector = 13 - 1;
                    break;
                case 77 * 2:
                    track = 35;
                    surface = 0;
                    sector = 23 - 1;
                    break;
                default:
                    track = 18;
                    surface = 1;
                    sector = 13 - 1;
                    break;
            }
        }

        private void getFatSector(out int track, out int surface, out int sectorFirst, out int sectorLast)
        {
            switch (AvailableTrack)
            {
                case 35:
                case 40:
                    track = 18;
                    surface = 0;
                    sectorFirst = 14 - 1;
                    sectorLast = 16 - 1;
                    break;
                case 77 * 2:
                    track = 35;
                    surface = 0;
                    sectorFirst = 24 - 1;
                    sectorLast = 26 - 1;
                    break;
                default:
                    track = 18;
                    surface = 1;
                    sectorFirst = 14 - 1;
                    sectorLast = 16 - 1;
                    break;
            }
        }

        private void getDirectorySector(out int track, out int surface, out int sectorFirst, out int sectorLast)
        {
            switch (AvailableTrack)
            {
                case 35:
                case 40:
                    track = 18;
                    surface = 0;
                    sectorFirst = 1 - 1;
                    sectorLast = 12 - 1;
                    break;
                case 77 * 2:
                    track = 35;
                    surface = 0;
                    sectorFirst = 1 - 1;
                    sectorLast = 22 - 1;
                    break;
                default:
                    track = 18;
                    surface = 1;
                    sectorFirst = 1-1;
                    sectorLast = 12 - 1;
                    break;
            }
        }

        private byte[] getFat()
        {
            int track, surface, sectorFirst, sectorLast;
            getFatSector(out track, out surface, out sectorFirst, out sectorLast);
            int maxClustors;
            if (AvailableTrack == 77 * 2)   // in 8inch drive?
                maxClustors = AvailableTrack; // track == cluster
            else
                maxClustors = AvailableTrack * 2; // track = 2cluster
            int offset, length;
            GetSectorDataOffsetAndLength(track, surface, sectorFirst, out offset, out length);
            var r = new byte[maxClustors];
            Array.Copy(diskImage, offset, r, 0, maxClustors);
            return r;
        }

        public IEnumerable<DirectoryEntry> EnumFiles()
        {
            int track, surface, sectorFirst, sectorLast;
            getDirectorySector(out track, out surface, out sectorFirst, out sectorLast);
            var fat = getFat();
            bool done = false;
            int id = 0;
            for (int sector = sectorFirst; sector <= sectorLast; sector++)
            {
                int offset, length;
                GetSectorDataOffsetAndLength(track, surface, sector, out offset, out length);
                for (int ent = 0; ent < 16; ent++)
                {
                    try
                    {
                        if (diskImage[offset + ent * 16] == 0) continue;
                        if (diskImage[offset + ent * 16] == 255) { done = true; break; }
                        yield return new DirectoryEntry(id, diskImage, offset + ent * 16);
                    }
                    finally
                    {
                        id++;
                    }
                }
                if (done) break;
            }
        }
        public VDisk(byte[] diskImage)
        {
            this.diskImage = diskImage;
            switch (this.diskImage.Length)
            {
                case 348848:
                    AvailableTrack = 80;
                    break;
                default:
                    throw new ApplicationException(this.diskImage.Length + " is not supported file size.");
            }
            switch (AvailableTrack)
            {
                case 35:
                case 40:
                case 80:
                    sectorsInTrack = 16;
                    sectorsInCluster = 8;
                    break;
                case 77 * 2:
                    sectorsInTrack = 26;
                    sectorsInCluster = 26;
                    break;
                default:
                    throw new ApplicationException(AvailableTrack + " is not supported tracks.");
            }
            MaxSurfaces = 2;
            if (AvailableTrack <= 40) MaxSurfaces = 1;
            sectorSizeWithMiscHeader = 256 + 16;
        }
    }
}
