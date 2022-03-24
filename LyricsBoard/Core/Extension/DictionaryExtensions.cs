using System;
using System.Collections.Generic;

namespace LyricsBoard.Core.Extension
{
    public static class DictionaryExtensions
    {
        public static TValue GetOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> self, TKey key, TValue defaultValue)
        {
            TValue value;
            return self.TryGetValue(key, out value) ? value : defaultValue;
        }
    }
}
