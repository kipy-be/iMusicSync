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
    }
}
