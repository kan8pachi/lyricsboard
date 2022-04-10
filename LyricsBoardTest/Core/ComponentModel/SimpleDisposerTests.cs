using LyricsBoard.Core.ComponentModel;
using Moq;
using System;
using Xunit;

namespace LyricsBoardTest.Core.ComponentModel
{
    public class SimpleDisposerTests
    {
        [Fact]
        public void Dispose_Work()
        {
            var disposer = new SimpleDisposer();
            var mock1 = new Mock<IDisposable>();
            var mock2 = new Mock<IDisposable>();

            disposer.Add(mock1.Object);
            mock2.Object.AddTo(disposer);

            disposer.Dispose();

            mock1.Verify(x => x.Dispose(), Times.Once());
            mock2.Verify(x => x.Dispose(), Times.Once());

            var mock3 = new Mock<IDisposable>();
            disposer.Add(mock3.Object);
            mock3.Verify(x => x.Dispose(), Times.Once(), "instance that is added after Dispose() should be disposed immediately");

            disposer.Dispose();

            mock1.Verify(x => x.Dispose(), Times.Once(), "2nd Dispose() should have no effect");
            mock2.Verify(x => x.Dispose(), Times.Once(), "2nd Dispose() should have no effect");
        }
    }
}
