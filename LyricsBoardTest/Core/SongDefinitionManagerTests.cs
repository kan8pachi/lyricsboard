using FluentAssertions;
using LyricsBoard.Core;
using Moq;
using Xunit;

namespace LyricsBoard.Test.Core
{
    public class SongDefinitionManagerTests
    {
        [Fact]
        public void GetSongDefinition_NotInCache()
        {
            var m = new Mock<ISongDefinitionLoader>();
            m.Setup(x => x.LoadByHash("hash")).Returns(new SongDefinition("hash"));

            var manager = new SongDefinitionManager(m.Object, 10);
            var actual = manager.GetSongDefinition("hash");
            
            actual.Should().NotBeNull();
            actual.SongHash.Should().Be("hash");

            m.VerifyAll();
        }

        [Fact]
        public void GetSongDefinition_AlreadyInCache()
        {
            var m = new Mock<ISongDefinitionLoader>();
            m.Setup(x => x.LoadByHash("hash")).Returns(new SongDefinition("hash"));
            m.Setup(x => x.LoadByHash("hash2")).Returns(new SongDefinition("hash2"));

            var manager = new SongDefinitionManager(m.Object, 10);
            manager.GetSongDefinition("hash");
            manager.GetSongDefinition("hash2");
            var actual = manager.GetSongDefinition("hash");

            actual.Should().NotBeNull();
            actual.SongHash.Should().Be("hash");

            //m.Verify(x => x.Load("hash"), Times.Once());
        }
    }
}
