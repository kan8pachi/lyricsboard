using FluentAssertions;
using LyricsBoard.Core;
using LyricsBoard.Core.System;
using Moq;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace LyricsBoard.Test.Core
{
    public class TimeMarkedTextTests
    {
        [Theory]
        [InlineData(2.99f)]
        [InlineData(7.01f)]
        public void GetProgress_OutOfRange(float input)
        {
            var text = new TimeMarkedText(3.0f, 5.0f, 7.0f, "text");
            var actual = text.GetProgress(input);

            actual.Should().BeNull();
        }

        [Theory]
        [InlineData(3.0f, 0f)]
        [InlineData(3.3f, 0.15f)]
        [InlineData(5.0f, 1f)]
        [InlineData(7.0f, 1f)]
        public void GetProgress_Work(float input, float expect)
        {
            var text = new TimeMarkedText(3.0f, 5.0f, 7.0f, "text");
            var actual = text.GetProgress(input);

            actual.Should().BeApproximately(expect, 0.0001f);
        }

        [Theory]
        [InlineData(3.0f, 1f)]
        [InlineData(3.3f, 1f)]
        [InlineData(7.0f, 1f)]
        public void GetProgress_NoAnimation(float input, float expect)
        {
            var text = new TimeMarkedText(3.0f, 3.0f, 7.0f, "text");
            var actual = text.GetProgress(input);

            actual.Should().BeApproximately(expect, 0.0001f);
        }
    }

    public class TimeMarkedTextListTests
    {
        public class PositionSearchTests
        {
            [Theory]
            [InlineData(2.0f, -1)]
            [InlineData(3.0f, 0)]
            [InlineData(4.0f, 0)]
            [InlineData(5.0f, 1)]
            [InlineData(6.0f, 1)]
            [InlineData(7.0f, 2)]
            [InlineData(8.0f, 2)]
            public void Search_Work(float input, int expect)
            {
                var search = new TimeMarkedTextList.PositionSearch(new List<float>() { 3.0f, 5.0f, 7.0f });
                var actual = search.Search(input);
                actual.Should().Be(expect);
            }
        }

        [Fact]
        public void Search_Empty()
        {
            var list = new TimeMarkedTextList(new List<TimeMarkedText>());
            var actual = list.Search(3f);

            actual.Progress.Should().BeNull();
            actual.Text.Should().BeEmpty();
        }

        [Fact]
        public void Search_BeforeFirst()
        {
            var list = new TimeMarkedTextList(new List<TimeMarkedText>()
            {
                new TimeMarkedText(13f, 15f, 17f, "text1"),
                new TimeMarkedText(23f, 25f, 27f, "text2")
            });
            var actual = list.Search(12.99f);

            actual.Progress.Should().BeNull();
            actual.Text.Should().BeEmpty();
        }

        [Fact]
        public void Search_AfterLast()
        {
            var list = new TimeMarkedTextList(new List<TimeMarkedText>()
            {
                new TimeMarkedText(13f, 15f, 17f, "text1"),
                new TimeMarkedText(23f, 25f, 27f, "text2")
            });
            var actual = list.Search(27.01f);

            actual.Progress.Should().BeNull();
            actual.Text.Should().BeEmpty();
        }

        [Theory]
        [InlineData(13f, 0f, "text1")]
        [InlineData(14f, 0.5f, "text1")]
        [InlineData(15f, 1f, "text1")]
        [InlineData(17f, 1f, "text1")]
        [InlineData(24f, 0.5f, "text2")]
        [InlineData(33f, 0f, "text3")]
        [InlineData(33.5f, 0.25f, "text3")]
        public void Search_Valid(float input, float expectProgress, string expectText)
        {
            var list = new TimeMarkedTextList(new List<TimeMarkedText>()
            {
                new TimeMarkedText(13f, 15f, 17f, "text1"),
                new TimeMarkedText(23f, 25f, 27f, "text2"),
                new TimeMarkedText(33f, 35f, 37f, "text3"),
            });
            var actual = list.Search(input);

            actual.Progress.Should().BeApproximately(expectProgress, 0.0001f);
            actual.Text.Should().Be(expectText);
        }

        [Theory]
        [InlineData(13f, 0f, "text1")]
        [InlineData(24f, 0.5f, "text2")]
        [InlineData(33.5f, 0.25f, "text3")]
        public void Search_NotOrdered(float input, float expectProgress, string expectText)
        {
            var list = new TimeMarkedTextList(new List<TimeMarkedText>()
            {
                new TimeMarkedText(33f, 35f, 37f, "text3"),
                new TimeMarkedText(23f, 25f, 27f, "text2"),
                new TimeMarkedText(13f, 15f, 17f, "text1"),
            });
            var actual = list.Search(input);

            actual.Progress.Should().BeApproximately(expectProgress, 0.0001f);
            actual.Text.Should().Be(expectText);
        }

        [Fact]
        public void Search_Continuous()
        {
            var list = new TimeMarkedTextList(new List<TimeMarkedText>()
            {
                new TimeMarkedText(33f, 35f, 37f, "text3"),
                new TimeMarkedText(23f, 25f, 27f, "text2"),
                new TimeMarkedText(13f, 15f, 17f, "text1"),
            });

            {
                var actual = list.Search(24f);
                actual.Progress.Should().BeApproximately(0.5f, 0.0001f);
                actual.Text.Should().Be("text2");
            }
            {
                // lastPos +0
                var actual = list.Search(24.2f);
                actual.Progress.Should().BeApproximately(0.6f, 0.0001f);
                actual.Text.Should().Be("text2");
            }
            {
                // lastPos +1
                var actual = list.Search(33.4f);
                actual.Progress.Should().BeApproximately(0.2f, 0.0001f);
                actual.Text.Should().Be("text3");
            }
            {
                // lastPos -2 (invalid)
                var actual = list.Search(13.2f);
                actual.Progress.Should().BeApproximately(0.1f, 0.0001f);
                actual.Text.Should().Be("text1");
            }
            {
                // lastPos +2 (invalid)
                var actual = list.Search(33.5f);
                actual.Progress.Should().BeApproximately(0.25f, 0.0001f);
                actual.Text.Should().Be("text3");
            }
        }
    }
}
