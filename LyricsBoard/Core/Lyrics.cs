﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LyricsBoard.Core
{
    internal record KaraokeTaggedChars(
        long TimeMs,
        string Text
    );

    internal record LyricsLine(
        long TimeMs,
        IEnumerable<KaraokeTaggedChars> Texts
    )
    {
        public string PlainText { get; } = Texts.Select(x => x.Text).Aggregate((a, b) => a + b);
    }

    internal record Lyrics(
        IEnumerable<LyricsLine> Lines
    );
}
