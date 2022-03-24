using System;
using System.Collections.Generic;

namespace LyricsBoard.Core.System
{
    internal class LRUMemoryCache<K, V>
        where K : notnull
        where V : class
    {
        protected readonly Dictionary<K, V> cache;
        protected readonly LinkedList<K> lru;
        protected readonly int capacity;

        public LRUMemoryCache(int capacity)
        {
            this.capacity = capacity;
            cache = new Dictionary<K, V>();
            lru = new LinkedList<K>();
        }

        /// <summary>
        /// Get object in cache if exist, otherwise create by invoke creator and cache it only if the result is not null.
        /// </summary>
        /// <returns>object in cache or created object. might be null if creator returns null.</returns>
        public V GetOrCreate(K key, Func<V> creator)
        {
            if (cache.ContainsKey(key))
            {
                lru.Remove(key);
                lru.AddLast(key);
                return cache[key];
            }

            var v = creator();
            //if (v == null)
            //{
            //    return null;
            //}
            Add(key, v);
            return v;
        }

        private void Add(K key, V entry)
        {
            lru.AddLast(key);
            cache.Add(key, entry);

            if (cache.Count > capacity)
            {
                RemoveLeastRecentlyUsed();
            }
        }

        private void RemoveLeastRecentlyUsed()
        {
            var key = lru.First.Value;
            lru.Remove(key);
            cache.Remove(key);
        }

        /// <summary>
        /// Clear cache.
        /// </summary>
        public void Clear()
        {
            lru.Clear();
            cache.Clear();
        }
    }
}
