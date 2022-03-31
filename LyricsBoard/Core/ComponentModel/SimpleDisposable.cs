using System;

namespace LyricsBoard.Core.ComponentModel
{
    internal class SimpleDisposable : IDisposable
    {
        private readonly Action disposeAction;
        public SimpleDisposable(Action disposeAction) => this.disposeAction = disposeAction;
        public void Dispose() => disposeAction?.Invoke();
    }

    internal class EmptyDisposable : IDisposable
    {
        public void Dispose() { }
    }
}
