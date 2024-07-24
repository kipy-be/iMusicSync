namespace iMusicSync.Data
{
    public class PlaylistItem
    {
        public string Title { get; set; }
        public string Artist { get; set; }
        public string FilePath { get; set; }
        public bool HasData => !string.IsNullOrWhiteSpace(FilePath);
    }
}
