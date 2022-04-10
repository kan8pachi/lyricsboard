using FluentAssertions;
using LyricsBoard.Core.ComponentModel;
using Moq;
using System;
using Xunit;

namespace LyricsBoardTest.Core.ComponentModel
{
    public class SimpleDisposableTests
    {
        [Fact]
        public void Dispose_Work()
        {
            var mock = new Mock<Action>();
            var disposable = new SimpleDisposable(mock.Object);
            disposable.IsDisposed.Should().BeFalse();

            disposable.Dispose();

            disposable.IsDisposed.Should().BeTrue();
            mock.Verify(x => x.Invoke(), Times.Once());
        }

        [Fact]
        public void Dispose_Twice()
        {
            var mock = new Mock<Action>();
            var disposable = new SimpleDisposable(mock.Object);
            disposable.IsDisposed.Should().BeFalse();

            disposable.Dispose();
            disposable.Dispose();

            disposable.IsDisposed.Should().BeTrue();
            mock.Verify(x => x.Invoke(), Times.Once());
        }
    }
}
