using iMusicSync.Data;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace iMusicSync.Services
{
    public class SyncService
    {
        private static MD5 _md5 = MD5.Create();

        public event EventHandler<SyncProgressEventArgs> OnSyncProgress;

        public void SyncTitles(List<PlaylistItem> playlist, string outputDir)
        {
            Task.Run(() =>
            {
                var titles = playlist
                    .Where(t => t.HasData)
                    .ToList();

                TitleSyncResult result;
                int skipCount = 0;
                int errorCount = 0;
                int syncCount = 0;
                int num = 0;


                foreach (var title in playlist.Where(t => t.HasData))
                {
                    result = SyncTitleToLocation(title, outputDir);

                    if (!result.IsSuccess)
                    {
                        errorCount++;
                    }
                    else
                    {
                        if (result.IsSkipped)
                        {
                            skipCount++;
                        }
                        else
                        {
                            syncCount++;
                        }
                    }

                    num++;
                    RaiseSyncProgressEvent(title, result, num, titles.Count, skipCount, syncCount, errorCount);
                }
            });
        }

        private TitleSyncResult SyncTitleToLocation(PlaylistItem title, string outputDir)
        {
            var result = new TitleSyncResult();

            if (!File.Exists(title.FilePath))
            {
                result.IsSuccess = false;
                result.Message = "Le fichier source n'existe pas";
                return result;
            }

            try
            {
                using (var fileIn = new FileStream(title.FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    string hash = BitConverter.ToString(_md5.ComputeHash(fileIn)).Replace("-", "").ToLowerInvariant();
                    string outputFilePath = Path.Combine(outputDir, $"{hash}{Path.GetExtension(title.FilePath)}");

                    if (File.Exists(outputFilePath))
                    {
                        result.IsSuccess = true;
                        result.IsSkipped = true;
                        return result;
                    }

                    using (var fileOut = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                    {
                        fileIn.Position = 0;
                        fileIn.CopyTo(fileOut);
                    }
                }

                result.IsSuccess = true;
                return result;
            }
            catch
            {
                result.IsSuccess = false;
                result.Message = "Erreur lors de la copie du fichier";
                return result;
            }
        }

        private void RaiseSyncProgressEvent(PlaylistItem playlistItem, TitleSyncResult result, int num, int totalCount, int skipCount, int syncCount, int errorCount)
        {
            OnSyncProgress?.Invoke(this, new SyncProgressEventArgs(playlistItem, result, num, totalCount, skipCount, syncCount, errorCount));
        }
    }
}
