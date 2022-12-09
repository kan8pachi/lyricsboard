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
            var mCatalog = new Mock<ISongCatalog>();
            mCatalog.Setup(x => x.LoadByHash("hash")).Returns(new SongDefinition("hash"));
            var mBuilder = new Mock<ISongCatalogBuilder>();
            mBuilder.Setup(x => x.Build()).Returns(mCatalog.Object);

            var manager = new SongDefinitionManager(null, mBuilder.Object, 10);
            manager.InitializeAsync().Wait();
            var actual = manager.GetSongDefinition("hash").Result;
            
            actual.Should().NotBeNull();
            actual.SongHash.Should().Be("hash");

            mBuilder.VerifyAll();
        }

        [Fact]
        public void GetSongDefinition_AlreadyInCache()
        {
            var mCatalog = new Mock<ISongCatalog>();
            mCatalog.Setup(x => x.LoadByHash("hash")).Returns(new SongDefinition("hash"));
            mCatalog.Setup(x => x.LoadByHash("hash2")).Returns(new SongDefinition("hash2"));
            var mBuilder = new Mock<ISongCatalogBuilder>();
            mBuilder.Setup(x => x.Build()).Returns(mCatalog.Object);

            var manager = new SongDefinitionManager(null, mBuilder.Object, 10);
            manager.InitializeAsync().Wait();
            _ = manager.GetSongDefinition("hash").Result;
            _ = manager.GetSongDefinition("hash2").Result;
            var actual = manager.GetSongDefinition("hash").Result;

            actual.Should().NotBeNull();
            actual.SongHash.Should().Be("hash");

            mCatalog.Verify(x => x.LoadByHash("hash"), Times.Once());
        }
    }
}
