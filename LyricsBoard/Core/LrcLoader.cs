using LyricsBoard.Core.Extension;
using LyricsBoard.Core.System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace LyricsBoard.Core
{
    internal class LrcLoader
    {
        private readonly Regex rxLine = new Regex(@"^\[(?<min>\d+):(?<sec>\d{1,2})([\.:](?<msc>\d{2}))?\](?<txt>.*)$");
        private readonly IFileSystem fs;
        private readonly string folder;

        // TODO: implement it. Always empty for now.
        public List<string> Errors { get; private set; }

        public LrcLoader(IFileSystem fs, string folder)
        {
            this.fs = fs;
            this.folder = folder;
            Errors = new List<string>();
        }

        /// <summary>
        /// Parse each line of LRC format, text with timestamp like "[mm:ss.:SS]text".
        /// </summary>
        /// <param name="line">line of text</param>
        /// <returns>Parsed instance or null if failed.</returns>
        public LyricsLine? ParseLine(string line)
        {
            var m = rxLine.Match(line);
            if (!m.Success) { return null; }

            var g = m.Groups;
            var smin = g["min"].Value;
            var ssec = g["sec"].Value;
            var smsc = g["msc"].Value;
            var stxt = g["txt"].Value;

            var min = PrimitiveParser.ParseIntOrDefault(smin, null);
            var sec = PrimitiveParser.ParseIntOrDefault(ssec, null);
            var msc = string.IsNullOrEmpty(smsc) ? 0 : PrimitiveParser.ParseIntOrDefault(smsc, null);

            // validate the range of the time.
            if (min == null)
            {
                return null;
            }
            if (sec == null || sec < 0 || sec > 59)
            {
                return null;
            }
            if (msc == null || msc < 0 || msc > 99)
            {
                return null;
            }

            long timeMs = (long)((min * 60 + sec) * 1000 + msc * 10);
            return new LyricsLine(timeMs, stxt);
        }

        /// <summary>
        /// Parse LRC format contents and convert to Lrc class.
        /// </summary>
        /// <param name="contents">Lines of text of LRC format contents. should not include line separator charactors.</param>
        /// <returns>Parsed instance. Never be null</returns>
        public Lyrics Parse(IEnumerable<string> contents)
        {
            var lrcLines = contents
                .Select(c => ParseLine(c))
                .WhereNotNull()
                .ToList();
            var lrc = new Lyrics(lrcLines);
            return lrc;
        }

        /// <summary>
        /// Read and parse LRC format file.
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns>Lrc instance, or null when file not found</returns>
        public Lyrics? LoadFromFileOrNull(string filename)
        {
            var filepath = Path.Combine(folder, filename);
            var lines = fs.ReatTextLinesOrNull(filepath);
            return lines is null ? null : Parse(lines);
        }
    }
}
