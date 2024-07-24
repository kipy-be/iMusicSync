using IMusicSyncException.Common;
using System;

namespace IMusicSync.Data
{
    public class DeviceInfo : Notifiable
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string Label => $"{Path.Replace("\\", "")} {Name} ({SpaceUsed}%)";
        public long Size { get; set; }
        public double SpaceUsed => Math.Round((100 - (double)AvailableSize / Size * 100), 2);

        private long _availableSize;
        public long AvailableSize
        {
            get => _availableSize;
            set => SetValue(ref _availableSize, value, nameof(AvailableSize), nameof(SpaceUsed), nameof(Label));
        }
    }
}
