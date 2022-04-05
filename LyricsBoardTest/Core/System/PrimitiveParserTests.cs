using FluentAssertions;
using LyricsBoard.Core.System;
using Xunit;

namespace LyricsBoard.Test.Core.System
{
    public class PrimitiveParserTests
    {
        [Fact]
        public void ParseLongOrDefault_Null()
        {
            {
                var actual = PrimitiveParser.ParseLongOrDefault(null, 99);
                actual.Should().Be(99);
            }
            {
                var actual = PrimitiveParser.ParseLongOrDefault(null, null);
                actual.Should().BeNull();
            }
        }

        [Fact]
        public void ParseLongOrDefault_Fail()
        {
            var actual = PrimitiveParser.ParseLongOrDefault("123.45", null);
            actual.Should().BeNull();
        }

        [Fact]
        public void ParseLongOrDefault_Success()
        {
            var actual = PrimitiveParser.ParseLongOrDefault("98746", null);
            actual.Should().Be(98746);
        }

        [Fact]
        public void ParseIntOrDefault_Null()
        {
            {
                var actual = PrimitiveParser.ParseIntOrDefault(null, 99);
                actual.Should().Be(99);
            }
            {
                var actual = PrimitiveParser.ParseIntOrDefault(null, null);
                actual.Should().BeNull();
            }
        }

        [Fact]
        public void ParseIntOrDefault_Fail()
        {
            var actual = PrimitiveParser.ParseIntOrDefault("123.45", null);
            actual.Should().BeNull();
        }

        [Fact]
        public void ParseIntOrDefault_Success()
        {
            var actual = PrimitiveParser.ParseIntOrDefault("98746", null);
            actual.Should().Be(98746);
        }
    }
}
