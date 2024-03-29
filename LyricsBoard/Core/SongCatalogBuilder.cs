﻿using LyricsBoard.Core.Extension;
using LyricsBoard.Core.System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace LyricsBoard.Core
{
    internal interface ISongCatalog
    {
        public SongDefinition LoadByHash(string songHash);
    }

    internal interface ISongCatalogBuilder
    {
        public ISongCatalog Build();
    }

    internal record SongCatalogEntry(string lrcFile, string? defFile);

    internal class SongCatalogBuilder : ISongCatalogBuilder
    {
        private readonly IFileSystem fs;
        private readonly IJson json;
        private readonly string folder;

        public SongCatalogBuilder(IFileSystem fs, IJson json, string folder)
        {
            this.fs = fs;
            this.json = json;
            this.folder = folder;
        }

        public ISongCatalog Build()
        {
            var newSongCatalog = new Dictionary<string, SongCatalogEntry>();
            var fileset = fs.EnumerateFilesAllWithExtPair(folder, ".lrc", ".json");
            foreach (var filepair in fileset)
            {
                var entry = new SongCatalogEntry(filepair.Item1, filepair.Item2);
                var hash = Path.GetFileNameWithoutExtension(filepair.Item1);
                newSongCatalog.Add(hash, entry);
            }
            return new SongCatalog(newSongCatalog, fs, json);
        }
    }

    class SongCatalog : ISongCatalog
    {
        private Dictionary<string, SongCatalogEntry> catalog;
        private readonly IFileSystem fs;
        private readonly IJson json;

        public SongCatalog(Dictionary<string, SongCatalogEntry> catalog, IFileSystem fs, IJson json)
        {
            this.catalog = catalog;
            this.fs = fs;
            this.json = json;
        }

        /// <summary>
        /// Load Lyrics def file and LRC file.
        /// When def file does not exist, just try to load LRC file.
        /// </summary>
        /// <returns>SongDefinition instance. Never be null.</returns>
        public SongDefinition LoadByHash(string songHash)
        {
            var catalogEntry = catalog.TryGetValue(songHash, out var result) ? result : null;
            if (catalogEntry == null)
            {
                return new SongDefinition(songHash);
            }

            // Load custom def file.
            var songDefinition = catalogEntry.defFile is null
                ? new SongDefinition(songHash)
                : LoadCustomDefinition(catalogEntry.defFile, songHash);

            // Load LRC file.
            var lrcLoader = new LrcLoader(fs);
            var lrc = lrcLoader.LoadFromFileOrNull(catalogEntry.lrcFile);

            return songDefinition with { Lyrics = lrc };
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
            var deserialized = json.DeserializeObjectOrDefault<Dictionary<string, string?>>(content);

            // dict sometimes be null. maybe when content is empty string?
            if (deserialized is null)
            {
                return new SongDefinition(songHash);
            }

            return GenerateCustomDefinitionSafely(
                songHash,
                PrimitiveParser.ParseIntOrDefault(deserialized?.GetOrDefault("OffsetMs", null), null),
                PrimitiveParser.ParseIntOrDefault(deserialized?.GetOrDefault("MaxExpirationMs", null), null),
                PrimitiveParser.ParseIntOrDefault(deserialized?.GetOrDefault("AnimationDurationMs", null), null),
                PrimitiveParser.ParseIntOrDefault(deserialized?.GetOrDefault("StandbyDurationMs", null), null)
            );
        }

        internal SongDefinition GenerateCustomDefinitionSafely(
            string songHash,
            int? offsetMs,
            int? maxExpirationMs,
            int? animationDurationMs,
            int? standbyDurationMs
        )
        {
            return new SongDefinition(
                songHash,
                MathN.ClampNullable(offsetMs, -3600 * 1000, 3600 * 1000),
                MathN.ClampNullable(maxExpirationMs, 100, 3600 * 1000),
                MathN.ClampNullable(animationDurationMs, 0, 60 * 1000),
                MathN.ClampNullable(standbyDurationMs, 0, 3600 * 1000),
                null
            );
        }
    }
}
