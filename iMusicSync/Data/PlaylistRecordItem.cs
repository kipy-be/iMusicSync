using CsvHelper.Configuration.Attributes;

namespace IMusicSync.Data
{
    public class PlaylistRecordItem
    {
        [Index(0)] public string Title { get; set; }
        [Index(1)] public string Artist { get; set; }
        [Index(2)] public string Composer { get; set; }
        [Index(3)] public string Album { get; set; }
        [Index(4)] public string Group { get; set; }
        [Index(5)] public string Piece { get; set; }
        [Index(6)] public string MouvementNum { get; set; }
        [Index(7)] public string MouvementCount { get; set; }
        [Index(8)] public string MouvementName { get; set; }
        [Index(9)] public string Genre { get; set; }
        [Index(10)] public string Size { get; set; }
        [Index(11)] public string Length { get; set; }
        [Index(12)] public string DiscNum { get; set; }
        [Index(13)] public string DiscCount { get; set; }
        [Index(14)] public string TrackNum { get; set; }
        [Index(15)] public string TrackCount { get; set; }
        [Index(16)] public string Year { get; set; }
        [Index(17)] public string UpdateDate { get; set; }
        [Index(18)] public string CreateDate { get; set; }
        [Index(19)] public string Bitrate { get; set; }
        [Index(20)] public string Frequency { get; set; }
        [Index(21)] public string Gain { get; set; }
        [Index(22)] public string Type { get; set; }
        [Index(23)] public string Equalizer { get; set; }
        [Index(24)] public string Comments { get; set; }
        [Index(25)] public string PlayCount { get; set; }
        [Index(26)] public string LastPlayDate { get; set; }
        [Index(27)] public string Jumps { get; set; }
        [Index(28)] public string LastJumpDate { get; set; }
        [Index(29)] public string Ranking { get; set; }
        [Index(30)] public string FilePath { get; set; }
    }
}
