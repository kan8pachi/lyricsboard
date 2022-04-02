using IPA.Utilities;
using LyricsBoard.Configuration;
using LyricsBoard.Core.System;
using SiraUtil.Logging;
using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace LyricsBoard.Core
{
    internal class LyricsBoardContext
    {
        private readonly SiraLog? logger;
        private readonly IFileSystem fs;
        private readonly SongDefinitionManager songManager;
        private readonly SongDefinition testSong;
        public PluginConfig Config { get; private set; }
        public bool IsWorking { get; private set; }

        public LyricsBoardContext(
            SiraLog? logger,
            PluginConfig? config,
            IFileSystem? fs
        )
        {
            this.logger = logger;

            // check if injected parameters are not null.
            if (config is null) { throw new ArgumentNullException(nameof(config)); }
            if (fs is null) { throw new ArgumentNullException(nameof(fs)); }
            Config = config;
            this.fs = fs;

            var rootFolder = UnityGame.UserDataPath;
            var ourFolder = Path.Combine(rootFolder, "LyricsBoard");
            var dataFolder = Path.Combine(ourFolder, "lyrics");

            testSong = SongDefinitionDummyGenerator.GenerateDummySong();
            var sdLoader = new SongDefinitionLoader(fs, dataFolder);
            songManager = new SongDefinitionManager(sdLoader, Config.CacheSize);

            var result = PrepareDataFolder(rootFolder, dataFolder);
            if (!result)
            {
                IsWorking = false;
                return;
            }

            IsWorking = true;
        }

        private bool AssertIfWorking([CallerMemberName] string? callerName = null)
        {
            if (!IsWorking)
            {
                if (callerName != null)
                {
                    logger?.Error($"[{callerName}] called when context is not working. Please contact the developer.");
                }
            }
            return IsWorking;
        }

        /// <summary>
        /// Make sure the data folder of our plugin exists.
        /// </summary>
        /// <returns>true when success, otherwise false.</returns>
        private bool PrepareDataFolder(string rootFolder, string dataFolder)
        {
            // Check if IPA plugin folder exists.
            if (rootFolder == null || !Directory.Exists(rootFolder))
            {
                var msg = $"User data directory which should be prepared by IPA was not found. ({rootFolder})";
                msg += " LyricsBoard is not working anymore.";
                logger?.Error(msg);
                return false;
            }

            // Create folder of our plugin directory if not exist.
            if (!fs.EnsureDirectoryIsPresent(dataFolder))
            {
                var msg = $"Failed to create our data folder. ({dataFolder})";
                msg += " LyricsBoard is not working any more.";
                logger?.Error(msg);
                return false;
            }

            return true;
        }

        private Lyrics? AdjustLyrics(SongDefinition sd)
        {
            if (sd.Lyrics == null) { return null; }
 
            var offset = Config.GetLrcAdditionalOffsetMs(sd.OffsetMs);
            var lines = sd.Lyrics.Lines
                .Select(x => new LyricsLine(x.TimeMs + offset, x.Text))
                .ToList();
            return new Lyrics(lines);
        }

        /// <summary>
        /// Get the lyrics animation calculator for the sond ID.
        /// </summary>
        /// <param name="songId">song hash. Must not be null.</param>
        /// <returns>calculator. may be null.</returns>
        public ProgressCalculator? GetLyricsProgressCalculator(string songId)
        {
            if (!AssertIfWorking()) { return null; }

            var sd = Config.ShowDebugLyrics
                ? testSong
                : songManager.GetSongDefinition(songId);
            if (sd is null) { return null; }

            var adjustedLyrics = AdjustLyrics(sd);
            if (adjustedLyrics is null) { return null; }

            var exirationMs = Config.GetLyricsLineMaxExpirationMs(sd.MaxExpirationMs);
            var animationMs = Config.GetLyricsAnimationDurationMs(sd.AnimationDurationMs);
            var standbyMs = Config.GetLyricsStandbyDurationMs(sd.StandbyDurationMs);

            var calculator = new ProgressCalculator(
                adjustedLyrics,
                new ProgressCalculator.Config(exirationMs / 1000f, animationMs / 1000f, standbyMs / 1000f)
            );
            return calculator;
        }

        public void ClearSongCache()
        {
            songManager.ClearSongCache();
        }
    }
}
