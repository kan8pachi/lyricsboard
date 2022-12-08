using FluentAssertions;
using LyricsBoard.Core;
using LyricsBoard.Core.System;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Xunit;

namespace LyricsBoard.Test.Core
{
    // Newtonsoft Json DLL included in BeatSaber game is modified to depend on UnityEngine.
    // Since i would like to make the test project independent of BeatSaber components,
    // implement IJson here by using pure Newtonsoft Json DLL.
    internal class UnityIndependentSilentJson : IJson
    {
        public T? DeserializeObjectOrDefault<T>(string value)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(value);
            }
            catch (Exception ex) when (ex is JsonReaderException || ex is JsonSerializationException)
            {
                return default;
            }
        }
    }

    public class SongCatalogTests
    {
        private readonly IJson json = new UnityIndependentSilentJson();
        private readonly string fullJson = "{OffsetMs:300, MaxExpirationMs:250, AnimationDurationMs:200, StandbyDurationMs:800}";

        [Theory]
        [InlineData("")]
        [InlineData("{}")]
        [InlineData("{{}")]
        [InlineData("{OffsetMs:300")]
        public void Parse_Empty(string input)
        {
            var catalog = new SongCatalog(new Dictionary<string, SongCatalogEntry>(), Mock.Of<IFileSystem>(), json);

            var actual = catalog.ParseCustomDefinition("hash", input);

            actual.Should().NotBeNull();
            actual.SongHash.Should().Be("hash");
            actual.OffsetMs.Should().BeNull();
            actual.MaxExpirationMs.Should().BeNull();
            actual.AnimationDurationMs.Should().BeNull();
            actual.StandbyDurationMs.Should().BeNull();
            actual.Lyrics.Should().BeNull();
        }

        [Fact]
        public void Parse_Full()
        {
            var catalog = new SongCatalog(new Dictionary<string, SongCatalogEntry>(), Mock.Of<IFileSystem>(), json);

            var actual = catalog.ParseCustomDefinition("hash", fullJson);

            actual.Should().NotBeNull();
            actual.SongHash.Should().Be("hash");
            actual.OffsetMs.Should().Be(300);
            actual.MaxExpirationMs.Should().Be(250);
            actual.AnimationDurationMs.Should().Be(200);
            actual.StandbyDurationMs.Should().Be(800);
            actual.Lyrics.Should().BeNull();
        }

        [Fact]
        public void LoadCustomDefinition_Work()
        {
            var mfs = new Mock<IFileSystem>();
            mfs.Setup(x => x.ReadTextAllOrEmpty("filepath")).Returns(fullJson);
            var catalog = new SongCatalog(new Dictionary<string, SongCatalogEntry>(), mfs.Object, json);

            var actual = catalog.LoadCustomDefinition("filepath", "hash");
            actual.SongHash.Should().Be("hash");
        }

        [Fact]
        public void LoadByHash_WithCustomDef()
        {
            var mfs = new Mock<IFileSystem>();
            mfs.Setup(x => x.ReadTextAllOrEmpty("rootfolder\\songhash.json")).Returns("{OffsetMs:300}");
            mfs.Setup(x => x.ReatTextLinesOrNull("rootfolder\\songhash.lrc")).Returns(new List<string>() { "[01:01]valid line 1" });
            mfs.Setup(x => x.EnumerateFilesAllWithExtPair("rootfolder", ".lrc", ".json")).Returns(new List<(string, string?)>() {
                ("rootfolder\\songhash.lrc", "rootfolder\\songhash.json")
            });
            var loader = new SongDefinitionLoader(mfs.Object, json, "rootfolder");
            var catalog = loader.BuildSongCatalog();

            var actual = catalog.LoadByHash("songhash");

            actual.SongHash.Should().Be("songhash");
            actual.OffsetMs.Should().Be(300);
            actual.Lyrics.Should().NotBeNull();
            actual.Lyrics!.Lines.Should().HaveCount(1);
            mfs.Verify(
                x => x.ReadTextAllOrEmpty(It.IsNotIn(new[] { "rootfolder\\songhash.json" })),
                Times.Never()
            );
            mfs.Verify(
                x => x.ReatTextLinesOrNull(It.IsNotIn(new[] { "rootfolder\\songhash.lrc" })),
                Times.Never()
            );
        }

        [Fact]
        public void LoadByHash_WithoutCustomDef()
        {
            var mfs = new Mock<IFileSystem>();
            mfs.Setup(x => x.ReadTextAllOrEmpty("rootfolder\\songhash.json")).Returns(string.Empty);
            mfs.Setup(x => x.ReatTextLinesOrNull("rootfolder\\songhash.lrc")).Returns(new List<string>() { "[01:01]valid line 1" });
            mfs.Setup(x => x.EnumerateFilesAllWithExtPair("rootfolder", ".lrc", ".json")).Returns(new List<(string, string?)>() {
                ("rootfolder\\songhash.lrc", null)
            });
            var loader = new SongDefinitionLoader(mfs.Object, json, "rootfolder");
            var catalog = loader.BuildSongCatalog();

            var actual = catalog.LoadByHash("songhash");

            actual.SongHash.Should().Be("songhash");
            actual.Lyrics.Should().NotBeNull();
            actual.Lyrics!.Lines.Should().HaveCount(1);
            mfs.Verify(
                x => x.ReadTextAllOrEmpty(It.IsNotIn(new[] { "rootfolder\\songhash.json" })),
                Times.Never()
            );
            mfs.Verify(
                x => x.ReatTextLinesOrNull(It.IsNotIn(new[] { "rootfolder\\songhash.lrc" })),
                Times.Never()
            );
        }

        [Fact]
        public void GenerateSafely_Fill()
        {
            var catalog = new SongCatalog(new Dictionary<string, SongCatalogEntry>(), Mock.Of<IFileSystem>(), json);
            var actual = catalog.GenerateCustomDefinitionSafely("songhash", 100, 200, 300, 400);

            actual.SongHash.Should().Be("songhash");
            actual.OffsetMs.Should().Be(100);
            actual.MaxExpirationMs.Should().Be(200);
            actual.AnimationDurationMs.Should().Be(300);
            actual.StandbyDurationMs.Should().Be(400);
        }

        [Fact]
        public void GenerateSafely_ClampUpper()
        {
            var catalog = new SongCatalog(new Dictionary<string, SongCatalogEntry>(), Mock.Of<IFileSystem>(), json);
            var actual = catalog.GenerateCustomDefinitionSafely("songhash", 3600001, 3600001, 60001, 3600001);

            actual.SongHash.Should().Be("songhash");
            actual.OffsetMs.Should().Be(3600000);
            actual.MaxExpirationMs.Should().Be(3600000);
            actual.AnimationDurationMs.Should().Be(60000);
            actual.StandbyDurationMs.Should().Be(3600000);
        }

        [Fact]
        public void GenerateSafely_ClampLower()
        {
            var catalog = new SongCatalog(new Dictionary<string, SongCatalogEntry>(), Mock.Of<IFileSystem>(), json);
            var actual = catalog.GenerateCustomDefinitionSafely("songhash", -3600001, 99, -1, -1);

            actual.SongHash.Should().Be("songhash");
            actual.OffsetMs.Should().Be(-3600000);
            actual.MaxExpirationMs.Should().Be(100);
            actual.AnimationDurationMs.Should().Be(0);
            actual.StandbyDurationMs.Should().Be(0);
        }
    }
}