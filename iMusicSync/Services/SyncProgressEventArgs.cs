using iMusicSync.Data;
using System;

namespace iMusicSync.Services
{
    public class SyncProgressEventArgs : EventArgs
    {
        public PlaylistItem PlaylistItem { get; private set; }
        public TitleSyncResult Result { get; private set; }
        public int Num { get; private set; }
        public int TotalCount { get; private set; }
        public int SkipCount { get; private set; }
        public int SyncCount { get; private set; }
        public int ErrorCount { get; private set; }

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
