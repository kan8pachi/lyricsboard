using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LyricsBoard.Core
{
    internal record LyricsLine(long TimeMs, string Text);

    internal record Lyrics(List<LyricsLine> Lines);
}
