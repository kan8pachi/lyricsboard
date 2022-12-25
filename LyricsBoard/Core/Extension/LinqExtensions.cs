using System;
using System.Collections;
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

        public static IEnumerable<T> AsEnumerable<T>(this IEnumerable collection)
        {
            foreach (var e in collection)
            {
                yield return (T)e;
            }
        }

        public static IEnumerable<TResult> Scan<T, TResult>(
            this IEnumerable<T> source,
            TResult seed,
            Func<TResult, T, TResult> func
        )
        {
            foreach (var x in source)
            {
                seed = func(seed, x);
                yield return seed;
            }
        }
    }
}
