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

    public class SongDefinitionLoaderTests
    {
        private readonly IJson json = new UnityIndependentSilentJson();
        private readonly string fullJson = "{OffsetMs:300, MaxExpirationMs:250, AnimationDurationMs:200, StandbyDurationMs:800, CustomLrcFileName:\"abc.lrc\"}";

        [Theory]
        [InlineData("")]
        [InlineData("{}")]
        [InlineData("{{}")]
        [InlineData("{OffsetMs:300")]
        public void Parse_Empty(string input)
        {
            var loader = new SongDefinitionLoader(Mock.Of<IFileSystem>(), json, "dummy");

            var actual = loader.ParseCustomDefinition("hash", input);

            actual.Should().NotBeNull();
            actual.SongHash.Should().Be("hash");
            actual.OffsetMs.Should().BeNull();
            actual.MaxExpirationMs.Should().BeNull();
            actual.AnimationDurationMs.Should().BeNull();
            actual.StandbyDurationMs.Should().BeNull();
            actual.CustomLrcFileName.Should().BeNull();
            actual.Lyrics.Should().BeNull();
        }

        [Fact]
        public void Parse_Full()
        {
            var loader = new SongDefinitionLoader(Mock.Of<IFileSystem>(), json, "dummy");

            var actual = loader.ParseCustomDefinition("hash", fullJson);

            actual.Should().NotBeNull();
            actual.SongHash.Should().Be("hash");
            actual.OffsetMs.Should().Be(300);
            actual.MaxExpirationMs.Should().Be(250);
            actual.AnimationDurationMs.Should().Be(200);
            actual.StandbyDurationMs.Should().Be(800);
            actual.CustomLrcFileName.Should().Be("abc.lrc");
            actual.Lyrics.Should().BeNull();
        }

        [Fact]
        public void LoadCustomDefinition_Work()
        {
            var mfs = new Mock<IFileSystem>();
            mfs.Setup(x => x.ReadTextAllOrEmpty("filepath")).Returns(fullJson);
            var loader = new SongDefinitionLoader(mfs.Object, json, "dummy");

            var actual = loader.LoadCustomDefinition("filepath", "hash");
            actual.SongHash.Should().Be("hash");
            actual.CustomLrcFileName.Should().Be("abc.lrc");
        }

        [Fact]
        public void LoadByHash_WithCustomLrc()
        {
            var mfs = new Mock<IFileSystem>();
            mfs.Setup(x => x.ReadTextAllOrEmpty("rootfolder\\songhash.json")).Returns(fullJson);
            mfs.Setup(x => x.ReatTextLinesOrNull("rootfolder\\abc.lrc")).Returns(new List<string>() { "[01:01]valid line 1" });
            var loader = new SongDefinitionLoader(mfs.Object, json, "rootfolder");

            var actual = loader.LoadByHash("songhash");

            actual.SongHash.Should().Be("songhash");
            actual.CustomLrcFileName.Should().Be("abc.lrc");
            actual.Lyrics!.Lines.Should().HaveCount(1);
            mfs.Verify(
                x => x.ReadTextAllOrEmpty(It.IsNotIn(new[] { "rootfolder\\songhash.json" })),
                Times.Never()
            );
            mfs.Verify(
                x => x.ReatTextLinesOrNull(It.IsNotIn(new[] { "rootfolder\\abc.lrc" })),
                Times.Never()
            );
        }

        [Fact]
        public void LoadByHash_WithoutCustomLrc()
        {
            var mfs = new Mock<IFileSystem>();
            mfs.Setup(x => x.ReadTextAllOrEmpty("rootfolder\\songhash.json")).Returns("{OffsetMs:300}");
            mfs.Setup(x => x.ReatTextLinesOrNull("rootfolder\\songhash.lrc")).Returns(new List<string>() { "[01:01]valid line 1" });
            var loader = new SongDefinitionLoader(mfs.Object, json, "rootfolder");

            var actual = loader.LoadByHash("songhash");

            actual.SongHash.Should().Be("songhash");
            actual.OffsetMs.Should().Be(300);
            actual.CustomLrcFileName.Should().BeNull();
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
            var loader = new SongDefinitionLoader(mfs.Object, json, "rootfolder");

            var actual = loader.LoadByHash("songhash");

            actual.SongHash.Should().Be("songhash");
            actual.CustomLrcFileName.Should().BeNull();
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
            var loader = new SongDefinitionLoader(Mock.Of<IFileSystem>(), json, "rootfolder");
            var actual = loader.GenerateCustomDefinitionSafely("songhash", 100, 200, 300, 400, "lrc");

            actual.SongHash.Should().Be("songhash");
            actual.OffsetMs.Should().Be(100);
            actual.MaxExpirationMs.Should().Be(200);
            actual.AnimationDurationMs.Should().Be(300);
            actual.StandbyDurationMs.Should().Be(400);
            actual.CustomLrcFileName.Should().Be("lrc");
        }

        [Fact]
        public void GenerateSafely_ClampUpper()
        {
            var loader = new SongDefinitionLoader(Mock.Of<IFileSystem>(), json, "rootfolder");
            var actual = loader.GenerateCustomDefinitionSafely("songhash", 3600001, 3600001, 60001, 3600001, "lrc");

            actual.SongHash.Should().Be("songhash");
            actual.OffsetMs.Should().Be(3600000);
            actual.MaxExpirationMs.Should().Be(3600000);
            actual.AnimationDurationMs.Should().Be(60000);
            actual.StandbyDurationMs.Should().Be(3600000);
            actual.CustomLrcFileName.Should().Be("lrc");
        }

        [Fact]
        public void GenerateSafely_ClampLower()
        {
            var loader = new SongDefinitionLoader(Mock.Of<IFileSystem>(), json, "rootfolder");
            var actual = loader.GenerateCustomDefinitionSafely("songhash", -3600001, 99, -1, -1, "lrc");

            actual.SongHash.Should().Be("songhash");
            actual.OffsetMs.Should().Be(-3600000);
            actual.MaxExpirationMs.Should().Be(100);
            actual.AnimationDurationMs.Should().Be(0);
            actual.StandbyDurationMs.Should().Be(0);
            actual.CustomLrcFileName.Should().Be("lrc");
        }
    }
}