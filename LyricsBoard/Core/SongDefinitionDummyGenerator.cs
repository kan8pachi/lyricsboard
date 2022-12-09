using System;
using System.Collections.Generic;
using System.Linq;

namespace LyricsBoard.Core
{
    internal static class SongDefinitionDummyGenerator
    {
        public static IEnumerable<T> Cumulate<T>(IEnumerable<T> e, Func<T, T, T> func, T defaultValue)
        {
            T val = defaultValue;
            foreach (var item in e)
            {
                val = func(val, item);
                yield return val;
            }
        }

        public static SongDefinition GenerateDummySong()
        {
            var intervals = new long[] { 2800, 2000, 1800, 800, 920, 1200, 300, 360, 230, 400, 1200, 9000, 11200, 5500, 1800 };
            var timings = Cumulate(intervals, (a, b) => a + b, 0);
            var lines = timings.Zip(intervals, (timing, interval) => (timing, interval))
                .Select(x => new LyricsLine(
                    x.timing,
                    new List<TimeTaggedText> {
                        new (x.timing, $"{x.interval} ms: Lrc text. にほんごも使えるよ. long long long text follows here.")
                    }
                ));
            return new SongDefinition("LyricsDummySong")
            {
                Lyrics = new Lyrics(lines),
            };

        }
    }
}
