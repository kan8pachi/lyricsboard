using Xunit;
using FluentAssertions;
using LyricsBoard.Core;

namespace LyricsBoard.Test.Core
{
    public class SongDefinitionTests
    {
        [Fact]
        public void SongDefinition_SongHashOnly()
        {
            var actual = new SongDefinition("songhash");

            actual.SongHash.Should().Be("songhash");
            actual.OffsetMs.Should().BeNull();
            actual.MaxExpirationMs.Should().BeNull();
            actual.AnimationDurationMs.Should().BeNull();
            actual.StandbyDurationMs.Should().BeNull();
            actual.Lyrics.Should().BeNull();
        }
    }
}
