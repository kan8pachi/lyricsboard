namespace LyricsBoard.Core
{
    internal record SongDefinition(
        string SongHash,
        int? OffsetMs,
        int? MaxExpirationMs,
        int? AnimationDurationMs,
        int? StandbyDurationMs,
        Lyrics? Lyrics
    )
    {
        public SongDefinition(string SongHash) : this(SongHash, null, null, null, null, null) { }
    }
}
