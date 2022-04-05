using FluentAssertions;
using LyricsBoard.Core.System;
using Xunit;

namespace LyricsBoard.Test.Core.System
{
    public class MathNTests
    {
        [Theory]
        [InlineData(14, 15)]
        [InlineData(15, 15)]
        [InlineData(20, 20)]
        [InlineData(21, 20)]
        public void ClampNullable_Work(int input, int expect)
        {
            var actual = MathN.ClampNullable(input, 15, 20);
            actual.Should().Be(expect);
        }

        [Fact]
        public void ClampNullable_Null()
        {
            var actual = MathN.ClampNullable(null, 15, 20);
            actual.Should().BeNull();
        }

        [Theory]
        [InlineData(14.999f, 15f)]
        [InlineData(15f, 15f)]
        [InlineData(20f, 20f)]
        [InlineData(20.001f, 20f)]
        public void Clamp_Work(float input, float expect)
        {
            var actual = MathN.Clamp(input, 15f, 20f);
            actual.Should().Be(expect);
        }

    }
}
