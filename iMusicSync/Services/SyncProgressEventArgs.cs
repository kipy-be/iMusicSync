using iMusicSync.Data;
using System;

namespace iMusicSync.Services
{
    public class SyncProgressEventArgs : EventArgs
    {
        public PlaylistItem PlaylistItem { get; set; }
        public TitleSyncResult Result { get; set; }
        public int Num { get; set; }
        public int TotalCount { get; set; }
        public int SkipCount { get; set; }
        public int SyncCount { get; set; }
        public int ErrorCount { get; set; }

        public SyncProgressEventArgs(PlaylistItem playlistItem, TitleSyncResult result, int num, int totalCount, int skipCount, int syncCount, int errorCount)
        {
            PlaylistItem = playlistItem;
            Result = result;
            Num = num;
            TotalCount = totalCount;
            SkipCount = skipCount;
            SyncCount = syncCount;
            ErrorCount = errorCount;
        }
    }
}
