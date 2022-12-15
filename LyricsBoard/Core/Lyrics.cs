using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LyricsBoard.Core
{
    internal record TimeTaggedChars(
        long TimeMs,
        string Text
    );

    internal record LyricsLine(
        long TimeMs,
        IEnumerable<TimeTaggedChars> Texts
    );

    internal record Lyrics(
        IEnumerable<LyricsLine> Lines
    );
}
