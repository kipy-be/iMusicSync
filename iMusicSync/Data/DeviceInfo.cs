using System;

namespace IMusicSync.Data
{
    public class DeviceInfo
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string Label => $"{Path.Replace("\\", "")} {Name} ({SpaceUsed}%)";
        public long AvailableSize { get; set; }
        public long Size { get; set; }
        public double SpaceUsed => Math.Round((100 - (double)AvailableSize / Size * 100), 2);
    }
}
