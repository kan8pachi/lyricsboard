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
                new LyricsLine(4000, new List<TimeTaggedChars> { new(4000,"text1") })
            });
            var config = new ProgressCalculator.Config(3f, 0.2f, 1f);
            var calc = new ProgressCalculator(lyrics, config);

            {
                var actual = calc.GetPresentProgress(2.799f);
                actual.Standby.Should().BeNull();
                actual.Current.Should().BeNull();
                actual.Retiring.Should().BeNull();
            }
            {
                var actual = calc.GetPresentProgress(2.8f);
                actual.Standby.Should().NotBeNull();
                actual.Standby!.Progress.Should().BeApproximately(0f, 0.0001f);
                actual.Standby.Data.Texts.Should().Contain(x => x.Text == "text1");
                actual.Current.Should().BeNull();
                actual.Retiring.Should().BeNull();
            }
            {
                var actual = calc.GetPresentProgress(2.9f);
                actual.Standby.Should().NotBeNull();
                actual.Standby!.Progress.Should().BeApproximately(0.5f, 0.0001f);
                actual.Standby.Data.Texts.Should().Contain(x => x.Text == "text1");
                actual.Current.Should().BeNull();
                actual.Retiring.Should().BeNull();
            }
            {
                var actual = calc.GetPresentProgress(3f);
                actual.Standby.Should().NotBeNull();
                actual.Standby!.Progress.Should().BeApproximately(1f, 0.0001f);
                actual.Standby.Data.Texts.Should().Contain(x => x.Text == "text1");
                actual.Current.Should().BeNull();
                actual.Retiring.Should().BeNull();
            }
            {
                var actual = calc.GetPresentProgress(3.799f);
                actual.Standby.Should().NotBeNull();
                actual.Standby!.Progress.Should().BeApproximately(1f, 0.0001f);
                actual.Standby.Data.Texts.Should().Contain(x => x.Text == "text1");
                actual.Current.Should().BeNull();
                actual.Retiring.Should().BeNull();
            }
            {
                var actual = calc.GetPresentProgress(3.800001f);
                actual.Standby.Should().BeNull();
                actual.Current.Should().NotBeNull();
                actual.Current!.Progress.Should().BeApproximately(0f, 0.0001f);
                actual.Current.Data.Texts.Should().Contain(x => x.Text == "text1");
                actual.Retiring.Should().BeNull();
            }
            {
                var actual = calc.GetPresentProgress(3.9f);
                actual.Standby.Should().BeNull();
                actual.Current.Should().NotBeNull();
                actual.Current!.Progress.Should().BeApproximately(0.5f, 0.0001f);
                actual.Current.Data.Texts.Should().Contain(x => x.Text == "text1");
                actual.Retiring.Should().BeNull();
            }
            {
                var actual = calc.GetPresentProgress(4f);
                actual.Standby.Should().BeNull();
                actual.Current.Should().NotBeNull();
                actual.Current!.Progress.Should().BeApproximately(1f, 0.0001f);
                actual.Current.Data.Texts.Should().Contain(x => x.Text == "text1");
                actual.Retiring.Should().BeNull();
            }
            {
                var actual = calc.GetPresentProgress(6.9999f);
                actual.Standby.Should().BeNull();
                actual.Current.Should().NotBeNull();
                actual.Current!.Progress.Should().BeApproximately(1f, 0.0001f);
                actual.Current.Data.Texts.Should().Contain(x => x.Text == "text1");
                actual.Retiring.Should().BeNull();
            }
            {
                var actual = calc.GetPresentProgress(7.000001f);
                actual.Standby.Should().BeNull();
                actual.Current.Should().BeNull();
                actual.Retiring.Should().NotBeNull();
                actual.Retiring!.Progress.Should().BeApproximately(0f, 0.0001f);
                actual.Retiring.Data.Texts.Should().Contain(x => x.Text == "text1");
            }
            {
                var actual = calc.GetPresentProgress(7.1f);
                actual.Standby.Should().BeNull();
                actual.Current.Should().BeNull();
                actual.Retiring.Should().NotBeNull();
                actual.Retiring!.Progress.Should().BeApproximately(0.5f, 0.0001f);
                actual.Retiring.Data.Texts.Should().Contain(x => x.Text == "text1");
            }
            {
                var actual = calc.GetPresentProgress(7.199999f);
                actual.Standby.Should().BeNull();
                actual.Current.Should().BeNull();
                actual.Retiring.Should().NotBeNull();
                actual.Retiring!.Progress.Should().BeApproximately(1f, 0.0001f);
                actual.Retiring.Data.Texts.Should().Contain(x => x.Text == "text1");
            }
            {
                var actual = calc.GetPresentProgress(7.2000001f);
                actual.Standby.Should().BeNull();
                actual.Current.Should().BeNull();
                actual.Retiring.Should().BeNull();
            }
        }
    }
}
