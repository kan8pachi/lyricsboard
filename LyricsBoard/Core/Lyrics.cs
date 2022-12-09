using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LyricsBoard.Core
{
    internal record TimeTaggedText(long TimeMs, string Text);
    internal record LyricsLine(long TimeMs, IEnumerable<TimeTaggedText> Texts);

    internal record Lyrics(IEnumerable<LyricsLine> Lines);
}
