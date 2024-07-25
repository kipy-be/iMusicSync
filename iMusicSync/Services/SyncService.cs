using iMusicSync.Data;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System;
using System.Linq;
using static TagLib.File;
using iMusicSync.Data.Context.Entities;
using iMusicSync.Data.Context;
using Microsoft.EntityFrameworkCore;
using System.IO.Pipes;

namespace iMusicSync.Services
{
    public class SyncService
    {
        private static string DB_FILE_DIR = ".imsdb";
        private static string DB_FILE_NAME = ".data";

        private static MD5 _md5 = MD5.Create();

        public event EventHandler<SyncProgressEventArgs> OnSyncProgress;
        public event EventHandler<SyncErrorEventArgs> OnSyncError;

        public void SyncTitles(List<PlaylistItem> playlist, string outputDir)
        {
            Task.Run(() =>
            {
                try
                {
                    var dbDir = Path.Combine(outputDir, DB_FILE_DIR);
                    var dbFile = Path.Combine(dbDir, DB_FILE_NAME);

                    Directory.CreateDirectory(dbDir);

                    using (var ctx = new MusicDbContext(dbFile))
                    {
                        ctx.Database.EnsureCreated();
                        SyncDbState(ctx, outputDir);

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
                            result = SyncTitleToLocation(ctx, title, outputDir);

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
                    }
                }
                catch (Exception ex)
                {
                    RaiseSyncErrorEvent($"Une erreur s'est produite pendant la synchronisation: {ex.Message}");
                }
            });
        }

        private void SyncDbState(MusicDbContext ctx, string outputDir)
        {
            var entriesIdsToDelete = new List<long>();

            foreach (var musicEntry in ctx.MusicEntries)
            {
                if (!File.Exists(musicEntry.Path))
                {
                    entriesIdsToDelete.Add(musicEntry.Id);
                }
            }

            if (entriesIdsToDelete.Count > 0)
            {
                ctx.MusicEntries.Where(me => entriesIdsToDelete.Contains(me.Id)).ExecuteDelete();
                ctx.SaveChanges();
            }

            var musicFiles = Directory.GetFiles(outputDir);

            if (musicFiles.Length != ctx.MusicEntries.Count())
            {
                foreach (var musicFile in musicFiles)
                {

                }
            }
        }

        private TitleSyncResult SyncTitleToLocation(MusicDbContext ctx, PlaylistItem title, string outputDir)
        {
            var result = new TitleSyncResult();

            if (!File.Exists(title.FilePath))
            {
                result.IsSuccess = false;
                result.Message = "Le fichier source n'existe pas";
                return result;
            }

            if (Path.GetExtension(title.FilePath).ToLower() == ".m4p")
            {
                result.IsSuccess = false;
                result.Message = "Le fichier est protégé par des DRM";
                return result;
            }

            Stream fileStream = null;

            try
            {
                var fileIn = Create(title.FilePath);
                fileStream = fileIn.FileAbstraction.ReadStream;

                var hash = BitConverter.ToString(_md5.ComputeHash(fileStream)).Replace("-", "").ToLowerInvariant();

                if (ctx.MusicEntries.Any(me => me.Hash == hash))
                {
                    result.IsSuccess = true;
                    result.IsSkipped = true;
                    return result;
                }
                
                var musicEntry = new MusicEntry()
                {
                    Album = fileIn.Tag.Album,
                    Title = fileIn.Tag.Title,
                    Artist = fileIn.Tag.FirstPerformer,
                    Duration = (long)fileIn.Properties.Duration.TotalSeconds,
                    Hash = hash
                };

                int i = 0;
                do
                {
                    musicEntry.Path = Path.Combine(outputDir, $"{musicEntry.Artist} - {musicEntry.Title}{(i > 0 ? $"_{i}" : "")}{Path.GetExtension(title.FilePath)}");
                    i++;
                } while (File.Exists(musicEntry.Path));
                

                ctx.MusicEntries.Add(musicEntry);
                ctx.SaveChanges();

                using (var fileOut = new FileStream(musicEntry.Path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                {
                    fileStream.Position = 0;
                    fileStream.CopyTo(fileOut);
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
            finally
            {
                if (fileStream != null)
                {
                    fileStream.Dispose();
                }
            }
        }

        private void RaiseSyncProgressEvent(PlaylistItem playlistItem, TitleSyncResult result, int num, int totalCount, int skipCount, int syncCount, int errorCount)
        {
            OnSyncProgress?.Invoke(this, new SyncProgressEventArgs(playlistItem, result, num, totalCount, skipCount, syncCount, errorCount));
        }

        private void RaiseSyncErrorEvent(string message)
        {
            OnSyncError?.Invoke(this, new SyncErrorEventArgs(message));
        }
    }
}
