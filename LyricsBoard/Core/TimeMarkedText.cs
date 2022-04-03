using LyricsBoard.Core.System;
using System.Collections.Generic;
using System.Linq;

namespace LyricsBoard.Core
{
    internal record TimeMarkedText(
        float StartTime,
        float AnimationEndTime,
        float EndTime,
        string Text
    );

    internal static class TimeMarkedTextExtensions
    {
        public static float? GetProgress(this TimeMarkedText self, float time)
        {
            if (time < self.StartTime || time > self.EndTime) {
                // Out of range should return zero.
                return null;
            }
            if (self.AnimationEndTime <= self.StartTime) {
                // no animation or invalid animation mean completed.
                return 1f;
            }
            var progress = (time - self.StartTime) / (self.AnimationEndTime - self.StartTime);
            return MathN.Clamp(progress, 0f, 1f);
        }
    }

    internal class TimeMarkedTextList
    {
        internal class PositionSearch
        {
            private readonly List<float> list;

            public PositionSearch(List<float> orderedList)
            {
                list = orderedList;
            }

            /// <summary>
            /// Find the position where val can be inserted, then return the index of the younger side.
            /// If the specified val is younger than the youngest in the list, returns -1.
            /// If the specified val is already in the list, returns its index.
            /// Assume list {3, 5, 7}:
            ///   2 returns -1
            ///   3 returns 0
            ///   4 returns 0
            ///   5 returns 1
            ///   6 returns 1
            ///   7 returns 2
            ///   8 returns 2
            /// </summary>
            /// <returns>The index of the position. Minimum possible index is -1. Maximum posibble index is the last index (count - 1).</returns>
            public int Search(float val)
            {
                var i = list.BinarySearch(val);
                if (i < 0) { i = ~i - 1; }
                return i;
            }
        }

        private readonly List<TimeMarkedText> sortedTexts;
        private readonly PositionSearch search;
        private readonly ProgressableText textCache;
        private int lastPos;
        public TimeMarkedTextList(List<TimeMarkedText> texts)
        {
            textCache = new ProgressableText();
            sortedTexts = texts.OrderBy(x => x.StartTime).ToList();

            var indexList = sortedTexts.Select(x => x.StartTime).ToList();
            search = new PositionSearch(indexList);

            lastPos = 0;
        }

        public ProgressableText Search(float time)
        {
            var pos = SearchPosition(time);
            if (pos < 0)
            {
                textCache.Progress = null;
                textCache.Text = string.Empty;
            }
            else
            {
                var ptext = sortedTexts[pos];
                var progress = ptext.GetProgress(time);
                textCache.Progress = progress;
                textCache.Text = progress == null ? string.Empty : ptext.Text;
            }
            return textCache;
        }

        private int SearchPosition(float time)
        {
            if (sortedTexts.Count == 0) { return -1; }

            // if the time is the same position or the next, then use it.
            if (IsValidPosition(time, lastPos)) { return lastPos; }
            if (IsValidPosition(time, lastPos + 1)) { return ++lastPos; }

            // search from whole list since the time might jumped.
            var pos = search.Search(time);
            lastPos = pos;
            return pos;
        }

        private bool IsValidPosition(float time, int pos)
        {
            var count = sortedTexts.Count;
            if (pos < 0 || pos >= count) { return false; }
            if (sortedTexts[pos].StartTime > time) { return false; }
            if (pos + 1 >= count) { return true; }
            if (sortedTexts[pos + 1].StartTime > time) { return true; }
            return false;
        }
    }
}
