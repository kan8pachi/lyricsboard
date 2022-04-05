using FluentAssertions;
using LyricsBoard.Core.System;
using Xunit;

namespace LyricsBoard.Test.Core.System
{
    public class LRUMemoryCacheTests
    {
        [Fact]
        public void Cache_Add_Work()
        {
            var cache = new LRUMemoryCache<string, string>(3);
            var actual1 = cache.GetOrCreate("key1", () => "valid value");
            actual1.Should().Be("valid value");

            var actual2 = cache.GetOrCreate("key1", () => "invalid value");
            actual2.Should().Be("valid value");
        }

        //[Fact]
        //public void Cache_Add_WhenNull()
        //{
        //    var cache = new LRUMemoryCache<string, string>(3);
        //    var actual1 = cache.GetOrCreate("key1", () => null);
        //    actual1.Should().BeNull();

        //    var actual2 = cache.GetOrCreate("key1", () => "valid value");
        //    actual2.Should().Be("valid value");
        //}

        [Fact]
        public void Cache_Capacity_Work()
        {
            var cache = new LRUMemoryCache<string, string>(2);
            cache.GetOrCreate("key1", () => "invalid");
            cache.GetOrCreate("key2", () => "dont care");

            // key1 will be removed here.
            cache.GetOrCreate("key3", () => "dont care");

            // check key1 is NOT in the cache.
            var actual = cache.GetOrCreate("key1", () => "valid");
            actual.Should().Be("valid");
        }

        [Fact]
        public void Cache_LRU_Work()
        {
            var cache = new LRUMemoryCache<string, string>(3);
            cache.GetOrCreate("key1", () => "value1");
            cache.GetOrCreate("key2", () => "dont care");
            cache.GetOrCreate("key3", () => "value3");

            // key1 used.
            cache.GetOrCreate("key1", () => "invalid");

            // least recently used key2 will be removed here.
            cache.GetOrCreate("key4", () => "dont care");

            // check key1 is in the cache.
            var actual1 = cache.GetOrCreate("key1", () => "invalid");
            actual1.Should().Be("value1");

            // check key3 is in the cache.
            var actual3 = cache.GetOrCreate("key3", () => "invalid");
            actual3.Should().Be("value3");
        }

        [Fact]
        public void Cache_Clear_Work()
        {
            var cache = new LRUMemoryCache<string, string>(3);
            cache.GetOrCreate("key1", () => "value1");
            cache.GetOrCreate("key2", () => "invalid");
            cache.GetOrCreate("key3", () => "invalid");

            var actual1 = cache.GetOrCreate("key1", () => "invalid");
            actual1.Should().Be("value1");

            // clear.
            cache.Clear();

            var actual2 = cache.GetOrCreate("key1", () => "valid");
            actual2.Should().Be("valid");
        }
    }
}
