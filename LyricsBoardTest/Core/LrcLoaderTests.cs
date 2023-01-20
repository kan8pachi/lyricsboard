using FluentAssertions;
using LyricsBoard.Core;
using LyricsBoard.Core.System;
using Moq;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace LyricsBoard.Test.Core
{
    public class LrcLoaderTests
    {
        [Theory]
        [InlineData("[01:02]short with zero", 62000, "short with zero")]
        [InlineData("[1:02]short min without zero", 62000, "short min without zero")]
        [InlineData("[1:2]short sec without zero", 62000, "short sec without zero")]
        [InlineData("[123:45]min with 3 digits", 7425000, "min with 3 digits")]
        [InlineData("[01:02.41]long dot", 62410, "long dot")]
        [InlineData("[01:02:41]long colon", 62410, "long colon")]
        [InlineData("[01:02.03]long dot with zero", 62030, "long dot with zero")]
        public void ParseLine_Valid(string input, long expectTime, string expectText)
        {
            var loader = new LrcLoader(Mock.Of<IFileSystem>());

            var actual = loader.ParseLine(input)!;
            actual.TimeMs.Should().Be(expectTime);
            actual.Texts.First().TimeMs.Should().Be(expectTime);
            actual.Texts.First().Text.Should().Be(expectText);
        }

        [Fact]
        public void ParseLine_TimeTagged()
        {
            var input = "[01:02]time-[01:08:24]tagged [01:15:90]text";
            var loader = new LrcLoader(Mock.Of<IFileSystem>());
            var actual = loader.ParseLine(input)!;
            actual.TimeMs.Should().Be(62000);
            actual.Texts.Should().HaveCount(3);
            actual.Texts.Select(x => x.TimeMs).Should().ContainInOrder(new[] { 62000L, 68240L, 75900L });
            actual.Texts.Select(x => x.Text).Should().ContainInOrder(new[] { "time-", "tagged ", "text" });
        }

        [Fact]
        public void ParseLine_TimeTaggedEmptyText()
        {
            var input = "[01:02][01:08]text";
            var loader = new LrcLoader(Mock.Of<IFileSystem>());
            var actual = loader.ParseLine(input)!;
            actual.TimeMs.Should().Be(62000);
            actual.Texts.Should().HaveCount(2);
            actual.Texts.Select(x => x.TimeMs).Should().ContainInOrder(new[] { 62000L, 68000L });
            actual.Texts.Select(x => x.Text).Should().ContainInOrder(new[] { "", "text" });
        }

        [Fact]
        public void ParseLine_TimeTaggedLastEmpty()
        {
            var input = "[01:02]text[01:08]";
            var loader = new LrcLoader(Mock.Of<IFileSystem>());
            var actual = loader.ParseLine(input)!;
            actual.TimeMs.Should().Be(62000);
            actual.Texts.Should().HaveCount(2);
            actual.Texts.Select(x => x.TimeMs).Should().ContainInOrder(new[] { 62000L, 68000L });
            actual.Texts.Select(x => x.Text).Should().ContainInOrder(new[] { "text", "" });
        }

        [Theory]
        [InlineData("[01.02]invalid colon")]
        [InlineData("[01:60]invalid sec value")]
        [InlineData("[01:59.100]invalid ms value")]
        [InlineData(" [01:02]contains leading whitespace")]
        [InlineData("[01:[02]syntax error")]
        public void ParseLine_Invalid(string input)
        {
            var loader = new LrcLoader(Mock.Of<IFileSystem>());

            var actual = loader.ParseLine(input);
            actual.Should().BeNull();
        }

        [Fact]
        public void Parse_NullExtracted()
        {
            var loader = new LrcLoader(Mock.Of<IFileSystem>());

            var input = new string[] {
                "invalid line",
                "[01:01]valid line 1",
                "02:02]invalid line",
                "[03:03]valid line 2",
                "[04:04.04]valid line 3",
            };
            var actual = loader.Parse(input);
            actual.Lines.Should().HaveCount(3);
        }

        [Fact]
        public void LoadFromFileOrNull_Work()
        {
            var mfs = new Mock<IFileSystem>();
            mfs.Setup(
                x => x.ReatTextLinesOrNull("folder\\testpath")
            ).Returns(
                new List<string>() { "[01:01]valid line 1" }
            );
            var loader = new LrcLoader(mfs.Object);

            var actual = loader.LoadFromFileOrNull("folder\\testpath")!;

            actual.Lines.Should().HaveCount(1);
            var first = actual.Lines.First();
            first.TimeMs.Should().Be(61000);
            mfs.Verify(
                x => x.ReatTextLinesOrNull(It.IsNotIn(new []{ "folder\\testpath" })),
                Times.Never()
            );
        }

        [Fact]
        public void LoadFromFileOrNull_Null()
        {
            var mfs = new Mock<IFileSystem>();
            mfs.Setup(
                x => x.ReatTextLinesOrNull("folder\\testpath")
            ).Returns(() => null);
            var loader = new LrcLoader(mfs.Object);

            var actual = loader.LoadFromFileOrNull("folder\\testpath");

            actual.Should().BeNull();
            mfs.Verify(
                x => x.ReatTextLinesOrNull(It.IsNotIn(new[] { "folder\\testpath" })),
                Times.Never()
            );
        }

    }
}
