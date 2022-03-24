using LyricsBoard.Core.Extension;
using LyricsBoard.Core.System;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace LyricsBoard.Core
{
    internal interface ISongDefinitionLoader
    {
        public SongDefinition LoadByHash(string songHash);
    }

    internal class SongDefinitionLoader : ISongDefinitionLoader
    {
        private readonly IFileSystem fs;
        private readonly string folder;

        public SongDefinitionLoader(IFileSystem fs, string folder)
        {
            this.fs = fs;
            this.folder = folder;
        }

        /// <summary>
        /// Load Lyrics def file and LRC file.
        /// When def file does not exist, just try to load LRC file.
        /// </summary>
        /// <returns>SongDefinition instance. Never be null.</returns>
        public SongDefinition LoadByHash(string songHash)
        {
            var defFilepath = Path.Combine(folder, $"{songHash}.json");

            // Load custom def file.
            var def = LoadCustomDefinition(defFilepath, songHash);

            // Load LRC file.
            var lrcFilename = def.CustomLrcFileName is null ? $"{songHash}.lrc" : def.CustomLrcFileName;
            var lrcLoader = new LrcLoader(fs, folder);
            var lrc = lrcLoader.LoadFromFileOrNull(lrcFilename);

            return def with { Lyrics = lrc };
        }

        /// <summary>
        /// Read ONLY custom def file. LRC file will not be read even if custom CustomLrcFileName is defined.
        /// </summary>
        /// <param name="filepath">The path to the custom def file.</param>
        /// <returns>SongDefinition instance. Never be null. (Lyrics property is always null.)</returns>
        internal SongDefinition LoadCustomDefinition(string filepath, string songHash)
        {
            var content = fs.ReadTextAllOrEmpty(filepath);
            return ParseCustomDefinition(songHash, content);

        }

        /// <summary>
        /// Parse custom def text.
        /// </summary>
        /// <param name="content">JSON text. It can be null or empty.</param>
        /// <returns>SongDefinition instance. Never be null.</returns>
        internal SongDefinition ParseCustomDefinition(string songHash, string content)
        {
            if (content == null)
            {
                return new SongDefinition(songHash);
            }

            // Parse JSON text.
            Dictionary<string, string?> dict;
            try
            {
                dict = JsonConvert.DeserializeObject<Dictionary<string, string?>>(content);
            }
            catch (Exception ex) when (ex is JsonReaderException || ex is JsonSerializationException)
            {
                return new SongDefinition(songHash);
            }

            // dict sometimes be null. maybe when content is empty string?
            if (dict is null)
            {
                return new SongDefinition(songHash);
            }

            return GenerateCustomDefinitionSafely(
                songHash,
                PrimitiveParser.ParseIntOrDefault(dict?.GetOrDefault("OffsetMs", null), null),
                PrimitiveParser.ParseIntOrDefault(dict?.GetOrDefault("MaxExpirationMs", null), null),
                PrimitiveParser.ParseIntOrDefault(dict?.GetOrDefault("AnimationDurationMs", null), null),
                PrimitiveParser.ParseIntOrDefault(dict?.GetOrDefault("StandbyDurationMs", null), null),
                dict?.GetOrDefault("CustomLrcFileName", null)
            );
        }

        internal SongDefinition GenerateCustomDefinitionSafely(
            string songHash,
            int? offsetMs,
            int? maxExpirationMs,
            int? animationDurationMs,
            int? standbyDurationMs,
            string? customLrcFileName
        )
        {
            return new SongDefinition(
                songHash,
                MathN.ClampNullable(offsetMs, -3600 * 1000, 3600 * 1000),
                MathN.ClampNullable(maxExpirationMs, 100, 3600 * 1000),
                MathN.ClampNullable(animationDurationMs, 0, 60 * 1000),
                MathN.ClampNullable(standbyDurationMs, 0, 3600 * 1000),
                customLrcFileName,
                null
            );
        }
    }
}
