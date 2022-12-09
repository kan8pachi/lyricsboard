using System;
using System.Collections.Generic;
using System.Linq;

namespace LyricsBoard.Core
{
    internal class ProgressableText
    {
        public float? Progress { get; set; }
        public string Text { get; set; } = string.Empty;
    }

    internal class ProgressCalculator
    {
        internal record Config(
            float ExpirationTime,
            float AnimationoDuration,
            float StandbyDuration
        );

        private readonly Lyrics lyrics;
        private readonly Config config;

        private Gen3Set<ProgressableText> textSetCache;
        private Gen3Set<TimeMarkedTextList> searcher;

        public ProgressCalculator(
            Lyrics lyrics,
            Config config
        )
        {
            this.lyrics = lyrics;
            this.config = config;

            textSetCache = new Gen3Set<ProgressableText>();
            searcher = BuildSearcher();
        }

        private class TimeSet
        {
            public float StandbyStart { get; set; }
            public float StandbyEnd { get; set; }
            public float CurrentStart { get; set; }
            public float CurrentEnd { get; set; }
            public float RetiringStart { get; set; }
            public float RetiringEnd { get; set; }
            public string Text { get; set; }

            public TimeSet(string text) { Text = text; }
        }

        private T? GetOrDefault<T>(List<T> list, int pos, T? defaultValue) where T : class
        {
            if (list == null || pos < 0 || pos >= list.Count)
            {
                return defaultValue;
            }
            return list[pos];
        }

        private Gen3Set<TimeMarkedTextList> BuildSearcher()
        {
            if(lyrics.Lines.Count() == 0)
            {
                var retiring = new TimeMarkedTextList(new List<TimeMarkedText>());
                var current = new TimeMarkedTextList(new List<TimeMarkedText>());
                var standby = new TimeMarkedTextList(new List<TimeMarkedText>());
                return new Gen3Set<TimeMarkedTextList>(standby, current, retiring);
            }

            var lines = lyrics.Lines
                .GroupBy(x => x.TimeMs)
                .Select(x => x.Last())
                .OrderBy(x => x.TimeMs)
                .Select(x =>
                    new TimeSet(string.Join(
                        "",
                        x.Texts.Select(ttt => ttt.Text)
                    ))
                    { CurrentEnd = x.TimeMs / 1000f }
                )
                .ToList();

            /**
             * The logic to determine each time.
             * - standby start : ([standby end] - animation-time)      or PREV [standby end] if later
             * - standby end   : ([current end] - standby - time)      or PREV [current end] if later
             * - current start : ([current end] - animation - time)]   or SELF [standby end] if later
             * - current end   : LRC time
             * - retiring start: ([current end] + expiration - time)   or NEXT [current start] if earlier
             * - retiring end  : ([retiring start] + animation - time) or NEXT [retiring start] if earlier
             **/

            // Determine [standby end], then [standby start] and [current start].
            for (int i = 0; i < lines.Count; i++)
            {
                var prev = GetOrDefault(lines, i - 1, null);
                var self = lines[i];

                var standbyEndCandidate = Math.Max(0, self.CurrentEnd - config.StandbyDuration);
                self.StandbyEnd = prev == null ? standbyEndCandidate : Math.Max(prev.CurrentEnd, standbyEndCandidate);

                var standbyStartCandidate = Math.Max(0, self.StandbyEnd - config.AnimationoDuration);
                self.StandbyStart = prev == null ? standbyStartCandidate : Math.Max(prev.StandbyEnd, standbyStartCandidate);

                var currentStartCandidate = Math.Max(0, self.CurrentEnd - config.AnimationoDuration);
                self.CurrentStart = Math.Max(self.StandbyEnd, currentStartCandidate);
            }

            // Determinie [retiring start] and [retiring end].
            for (int i = lines.Count - 1; i >= 0; i--)
            {
                var next = GetOrDefault(lines, i + 1, null);
                var self = lines[i];

                var retiringStartCandidate = self.CurrentEnd + config.ExpirationTime;
                self.RetiringStart = next == null ? retiringStartCandidate : Math.Min(next.CurrentStart, retiringStartCandidate);

                var retiringEndCandidate = self.RetiringStart + config.AnimationoDuration;
                self.RetiringEnd = next == null ? retiringEndCandidate : Math.Min(next.RetiringStart, retiringEndCandidate);
            }

            var standbyLines = lines
                .Select(x => new TimeMarkedText(x.StandbyStart, x.StandbyEnd, x.CurrentStart, x.Text))
                .ToList();
            var currentLines = lines
                .Select(x => new TimeMarkedText(x.CurrentStart, x.CurrentEnd, x.RetiringStart, x.Text))
                .ToList();
            var retiringLines = lines
                .Select(x => new TimeMarkedText(x.RetiringStart, x.RetiringEnd, x.RetiringEnd, x.Text))
                .ToList();

            return new Gen3Set<TimeMarkedTextList>(
                new TimeMarkedTextList(standbyLines),
                new TimeMarkedTextList(currentLines),
                new TimeMarkedTextList(retiringLines)
            );
        }

        public Gen3Set<ProgressableText> GetPresentProgress(float songTime)
        {
            textSetCache.Standby = searcher.Standby.Search(songTime);
            textSetCache.Current = searcher.Current.Search(songTime);
            textSetCache.Retiring = searcher.Retiring.Search(songTime);
            return textSetCache;
        }
    }
}
