using LyricsBoard.Core.Extension;
using LyricsBoard.Core.System;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace LyricsBoard.Core
{
    internal class LrcLoader
    {
        private readonly Regex exHead = new(@"^\[(\d+):(\d{1,2})([\.:](\d{2}))?\]");
        private readonly Regex exMid = new(@"\[(?<min>\d+):(?<sec>\d{1,2})([\.:](?<msc>\d{2}))?\](?<txt>.*?)(?=\[(\d+):(\d{1,2})([\.:](\d{2}))?\])");
        private readonly Regex exTail = new(@"\[(?<min>\d+):(?<sec>\d{1,2})([\.:](?<msc>\d{2}))?\](?<txt>.*?)$", RegexOptions.RightToLeft);
        private readonly IFileSystem fs;

        // TODO: implement it. Always empty for now.
        public List<string> Errors { get; private set; }

        public LrcLoader(IFileSystem fs)
        {
            this.fs = fs;
            Errors = new List<string>();
        }

        /// <summary>
        /// Parse each line of LRC format, text with timestamp like "[mm:ss.:SS]text".
        /// </summary>
        /// <param name="line">line of text</param>
        /// <returns>Parsed instance or null if failed.</returns>
        public LyricsLine? ParseLine(string line)
        {
            Func<GroupCollection, TimeTaggedText?> f = (group) =>
            {
                var smin = group["min"].Value;
                var ssec = group["sec"].Value;
                var smsc = group["msc"].Value;
                var stxt = group["txt"].Value;

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
                return new TimeTaggedText(timeMs, stxt);
            };

            // skip if line doesn't start from time tag.
            var mHead = exHead.Match(line);
            if (!mHead.Success)
            {
                return null;
            }

            var mTail = exTail.Match(line);
            if (!mTail.Success)
            {
                return null;
            }
            var resultTail = f(mTail.Groups);

            var mMid = exMid.Matches(line);
            var result = mMid.AsEnumerable<Match>()
                .Select(x => f(x.Groups))
                .Append(resultTail)
                .WhereNotNull();

            var resultFirst = result.FirstOrDefault();
            if(resultFirst is null)
            {
                return null;
            }
            return new LyricsLine(resultFirst.TimeMs, result);
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
        public Lyrics? LoadFromFileOrNull(string filepath)
        {
            var lines = fs.ReatTextLinesOrNull(filepath);
            return lines is null ? null : Parse(lines);
        }
    }
}
