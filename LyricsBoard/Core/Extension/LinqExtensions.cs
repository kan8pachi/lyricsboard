using System;
using System.Collections.Generic;
using System.Linq;

namespace LyricsBoard.Core.Extension
{
    public static class LinqExtensions
    {
        public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> that) where T : struct
            => that.Where(x => x.HasValue).Select(x => x ?? default);

        public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> that) where T : class
            => that.Where(x => x != null)!;
    }
}
