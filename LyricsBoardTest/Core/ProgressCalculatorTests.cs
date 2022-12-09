using FluentAssertions;
using LyricsBoard.Core;
using System.Collections.Generic;
using Xunit;

namespace LyricsBoard.Test.Core
{
    public class ProgressCalculatorTests
    {
        [Fact]
        public void GetPresentProgress_Single()
        {
            var lyrics = new Lyrics(new List<LyricsLine>() {
                new LyricsLine(4000, new List<TimeTaggedText> { new(4000,"text1") })
            });
            var config = new ProgressCalculator.Config(3f, 0.2f, 1f);
            var calc = new ProgressCalculator(lyrics, config);

            {
                var actual = calc.GetPresentProgress(2.799f);
                actual.Standby.Progress.Should().BeNull();
                actual.Standby.Text.Should().BeEmpty();
                actual.Current.Progress.Should().BeNull();
                actual.Current.Text.Should().BeEmpty();
                actual.Retiring.Progress.Should().BeNull();
                actual.Retiring.Text.Should().BeEmpty();
            }
            {
                var actual = calc.GetPresentProgress(2.8f);
                actual.Standby.Progress.Should().BeApproximately(0f, 0.0001f);
                actual.Standby.Text.Should().Be("text1");
                actual.Current.Progress.Should().BeNull();
                actual.Current.Text.Should().BeEmpty();
                actual.Retiring.Progress.Should().BeNull();
                actual.Retiring.Text.Should().BeEmpty();
            }
            {
                var actual = calc.GetPresentProgress(2.9f);
                actual.Standby.Progress.Should().BeApproximately(0.5f, 0.0001f);
                actual.Standby.Text.Should().Be("text1");
                actual.Current.Progress.Should().BeNull();
                actual.Current.Text.Should().BeEmpty();
                actual.Retiring.Progress.Should().BeNull();
                actual.Retiring.Text.Should().BeEmpty();
            }
            {
                var actual = calc.GetPresentProgress(3f);
                actual.Standby.Progress.Should().BeApproximately(1f, 0.0001f);
                actual.Standby.Text.Should().Be("text1");
                actual.Current.Progress.Should().BeNull();
                actual.Current.Text.Should().BeEmpty();
                actual.Retiring.Progress.Should().BeNull();
                actual.Retiring.Text.Should().BeEmpty();
            }
            {
                var actual = calc.GetPresentProgress(3.799f);
                actual.Standby.Progress.Should().BeApproximately(1f, 0.0001f);
                actual.Standby.Text.Should().Be("text1");
                actual.Current.Progress.Should().BeNull();
                actual.Current.Text.Should().BeEmpty();
                actual.Retiring.Progress.Should().BeNull();
                actual.Retiring.Text.Should().BeEmpty();
            }
            {
                var actual = calc.GetPresentProgress(3.800001f);
                actual.Standby.Progress.Should().BeNull();
                actual.Standby.Text.Should().BeEmpty();
                actual.Current.Progress.Should().BeApproximately(0f, 0.0001f);
                actual.Current.Text.Should().Be("text1");
                actual.Retiring.Progress.Should().BeNull();
                actual.Retiring.Text.Should().BeEmpty();
            }
            {
                var actual = calc.GetPresentProgress(3.9f);
                actual.Standby.Progress.Should().BeNull();
                actual.Standby.Text.Should().BeEmpty();
                actual.Current.Progress.Should().BeApproximately(0.5f, 0.0001f);
                actual.Current.Text.Should().Be("text1");
                actual.Retiring.Progress.Should().BeNull();
                actual.Retiring.Text.Should().BeEmpty();
            }
            {
                var actual = calc.GetPresentProgress(4f);
                actual.Standby.Progress.Should().BeNull();
                actual.Standby.Text.Should().BeEmpty();
                actual.Current.Progress.Should().BeApproximately(1f, 0.0001f);
                actual.Current.Text.Should().Be("text1");
                actual.Retiring.Progress.Should().BeNull();
                actual.Retiring.Text.Should().BeEmpty();
            }
            {
                var actual = calc.GetPresentProgress(6.9999f);
                actual.Standby.Progress.Should().BeNull();
                actual.Standby.Text.Should().BeEmpty();
                actual.Current.Progress.Should().BeApproximately(1f, 0.0001f);
                actual.Current.Text.Should().Be("text1");
                actual.Retiring.Progress.Should().BeNull();
                actual.Retiring.Text.Should().BeEmpty();
            }
            {
                var actual = calc.GetPresentProgress(7.000001f);
                actual.Standby.Progress.Should().BeNull();
                actual.Standby.Text.Should().BeEmpty();
                actual.Current.Progress.Should().BeNull();
                actual.Current.Text.Should().BeEmpty();
                actual.Retiring.Progress.Should().BeApproximately(0f, 0.0001f);
                actual.Retiring.Text.Should().Be("text1");
            }
            {
                var actual = calc.GetPresentProgress(7.1f);
                actual.Standby.Progress.Should().BeNull();
                actual.Standby.Text.Should().BeEmpty();
                actual.Current.Progress.Should().BeNull();
                actual.Current.Text.Should().BeEmpty();
                actual.Retiring.Progress.Should().BeApproximately(0.5f, 0.0001f);
                actual.Retiring.Text.Should().Be("text1");
            }
            {
                var actual = calc.GetPresentProgress(7.199999f);
                actual.Standby.Progress.Should().BeNull();
                actual.Standby.Text.Should().BeEmpty();
                actual.Current.Progress.Should().BeNull();
                actual.Current.Text.Should().BeEmpty();
                actual.Retiring.Progress.Should().BeApproximately(1f, 0.0001f);
                actual.Retiring.Text.Should().Be("text1");
            }
            {
                var actual = calc.GetPresentProgress(7.2000001f);
                actual.Standby.Progress.Should().BeNull();
                actual.Standby.Text.Should().BeEmpty();
                actual.Current.Progress.Should().BeNull();
                actual.Current.Text.Should().BeEmpty();
                actual.Retiring.Progress.Should().BeNull();
                actual.Retiring.Text.Should().BeEmpty();
            }
        }
    }
}
