using CsvHelper;
using CsvHelper.Configuration;
using iMusicSync.Data;
using iMusicSync.Services;
using IMusicSync.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace IMusicSync.Services
{
    public class PlaylistService
    {
        public event EventHandler<SyncProgressEventArgs> OnSyncProgress;

        private CsvConfiguration _csvConfig = new (CultureInfo.InvariantCulture)
        {
            NewLine = Environment.NewLine,
            HasHeaderRecord = true,
            Delimiter = "\t",
            BadDataFound = null,
            MissingFieldFound = null
        };

        public List<PlaylistItem> ReadPlaylistFile(string fileUri)
        {
            var playlist = new List<PlaylistItem>();

            using (var reader = new StreamReader(new FileStream(fileUri, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
            using (var csv = new CsvReader(reader, _csvConfig))
            {
                csv.Read();
                csv.ReadHeader();

                while (csv.Read())
                {
                    var record = csv.GetRecord<PlaylistRecordItem>();

                    playlist.Add(new PlaylistItem
                    {
                        Artist = record.Artist,
                        Title = record.Title,
                        FilePath = record.FilePath
                    });
                }
            }

            return playlist;
        }

        public List<PlaylistItem> ToPlaylist(string[] filesUri)
        {
            var playlist = new List<PlaylistItem>();

            foreach (string fileUri in filesUri)
            {
                playlist.Add(new PlaylistItem
                {
                    FilePath = fileUri
                });
            }

            return playlist;
        }

        private static MD5 _md5 = MD5.Create();

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
