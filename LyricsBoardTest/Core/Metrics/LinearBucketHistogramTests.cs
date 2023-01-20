using FluentAssertions;
using LyricsBoard.Core.Metrics;
using System.Linq;
using Xunit;

namespace LyricsBoardTest.Core.Metrics
{
    public class LinearBucketHistogramTests
    {
        [Fact]
        public void Basic_Work()
        {
            var histogram = new LinearBucketHistogram("test", 10, 0, 10);
            histogram.Record(29);
            histogram.Record(13);
            histogram.Record(141);
            histogram.Record(29);

            histogram.Name.Should().Be("test");
            histogram.Count.Should().Be(4);
            histogram.Min.Should().Be(13);
            histogram.Max.Should().Be(141);
        }

        [Fact]
        public void Percentile()
        {
            var histogram = new LinearBucketHistogram("test", 10, 0, 10);

            histogram.Record(-1); // #1
            for (int i = 0; i < 99; i++)
            {
                histogram.Record(0);    // #100
                histogram.Record(11);   // #199
                histogram.Record(21);   // #298
                histogram.Record(31);   // #397
                histogram.Record(41);   // #496
                histogram.Record(51);   // #595
                histogram.Record(61);   // #694
                histogram.Record(71);   // #793
                histogram.Record(81);   // #892
                histogram.Record(99);   // #991
            }
            for (int i = 0; i < 9; i++)
            {
                histogram.Record(100);  // #1000
            }

            var percentile = histogram.GetPercentile(new int[] { 0, 100, 1, 99, 10, 90, 25, 75, 50 });
            percentile.Should().HaveCount(9);
            percentile.Select(x => x.Item1).Should().ContainInOrder(new int[] { 0, 1, 10, 25, 50, 75, 90, 99, 100 });
            percentile.Select(x => x.Item2).Should().ContainInOrder(new int[] { -1, 0, 0, 20, 50, 70, 90, 90, 100 });
        }

    }
}
