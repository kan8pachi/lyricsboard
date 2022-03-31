using LyricsBoard.Core.System;

namespace LyricsBoard.Core
{
    /// <summary>
    /// The public interface for SondDefinition users.
    /// </summary>
    internal class SongDefinitionManager
    {
        private readonly LRUMemoryCache<string, SongDefinition> songCache;
        private readonly ISongDefinitionLoader sdLoader;

        public SongDefinitionManager(ISongDefinitionLoader sdLoader, int cacheCapacity)
        {
            songCache = new LRUMemoryCache<string, SongDefinition>(cacheCapacity);
            this.sdLoader = sdLoader;
        }

        /// <summary>
        /// Return SongDefinition from cache. If not found, load from file and cache it, and return it.
        /// </summary>
        /// <param name="songHash">Song hash. Must not be null.</param>
        /// <returns>Never be null. If not found, Lyrics property will be null.</returns>
        public SongDefinition GetSongDefinition(string songHash)
        {
            var sd = songCache.GetOrCreate(
                songHash,
                () => sdLoader.LoadByHash(songHash)
            );
            return sd;
        }

        public void ClearSongCache()
        {
            songCache.Clear();
        }
    }
}
