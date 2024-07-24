using System.IO;

namespace iMusicSync.Data
{
    public class PlaylistItem
    {
        public string Title { get; set; }
        public string Artist { get; set; }
        public string FilePath { get; set; }
        public string FileName => Path.GetFileName(FilePath);
        public bool HasData => !string.IsNullOrWhiteSpace(FilePath);

        public string Label()
        {
            if (Artist != null && Title != null)
            {
                return $"{Artist} - {Title}";
            }

            return FileName;
        }
    }
}
