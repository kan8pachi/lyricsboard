using LyricsBoard.Core.Extension;
using System.Collections.Generic;
using System.Linq;

namespace LyricsBoard.Core.Metrics
{
    internal class LinearBucketHistogram
    {
        private readonly int bucketNum;
        private readonly int bucketMinValue;
        private readonly int bucketDevider;
        private readonly long[] buckets;
        private long bucketUnderflow;
        private long bucketOverflow;

        public long Count { get; private set; }
        public long Min { get; private set; }
        public long Max { get; private set; }
        public string Name { get; private set; }

        public LinearBucketHistogram(string name, int bucketNum, int bucketMinValue, int bucketDevider)
        {
            Name = name;
            this.bucketNum = bucketNum;
            this.bucketMinValue = bucketMinValue;
            this.bucketDevider = bucketDevider;
            buckets = new long[bucketNum];
            Min = int.MaxValue;
            Max = int.MinValue;
        }

        public void Record(long value)
        {
            Count++;

            if (value < Min)
            {
                Min = value;
            }
            else if (value > Max)
            {
                Max = value;
            }

            var bucketIndex = (value - bucketMinValue) / bucketDevider;

            if (bucketIndex < 0)
            {
                bucketUnderflow++;
            }
            if (bucketIndex >= bucketNum)
            {
                bucketOverflow++;
            }
            else
            {
                buckets[bucketIndex]++;
            }
        }

        private long ArrayAt(long[] array, int index, long underflow)
        {
            return index == -1 ? underflow : array[index];
        }

        public IEnumerable<(int, int)> GetPercentile(IEnumerable<int> percentiles)
        {
            var bucketsAccumulated = buckets
                .Scan(bucketUnderflow, (a, b) => a + b)
                .ToArray();

            int bucketIndex = -1;
            var results = percentiles
                .Where(x => x >= 0 && x <= 100)
                .OrderBy(x => x)
                .Select(x =>
                    {
                        var threadshold = Count * x / 100;
                        for (; bucketIndex < bucketNum; bucketIndex++)
                        {
                            if (threadshold <= ArrayAt(bucketsAccumulated, bucketIndex, bucketUnderflow))
                            {
                                break;
                            }
                        }
                        var bucketValue = bucketIndex < 0 ? -1 : bucketIndex * bucketDevider + bucketMinValue;
                        return (x, bucketValue);
                    }
                ).ToArray();

            return results;
        }

        public string Summary()
        {
            var percentiles = GetPercentile(new int[] { 1, 10, 50, 90, 99 });
            var result = $"[{Name}] count:{Count}, min:{Min}, Max:{Max}, underflow:{bucketUnderflow}, overflow:{bucketOverflow}";
            result += percentiles.Aggregate("", (a, b) => a + $" {b.Item1}p:{b.Item2}");
            return result;
        }
    }
}
