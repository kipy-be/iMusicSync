using System;

namespace iMusicSync.Services
{
    public class SyncErrorEventArgs : EventArgs
    {
        public string Message { get; private set; }

        public SyncErrorEventArgs(string message)
        {
            Message = message;
        }
    }
}
