using System;

namespace LyricsBoard.Core.ComponentModel
{
    internal class SimpleDisposable : IDisposable
    {
        private volatile Action? disposeAction;

        public SimpleDisposable(Action disposeAction)
        {
            this.disposeAction = disposeAction;
        }

        public bool IsDisposed
        {
            get { return disposeAction is null; }
        }

        public void Dispose()
        {
            if (disposeAction is null) { return; }
            disposeAction.Invoke();
            disposeAction = null;
        }
    }

    internal class EmptyDisposable : IDisposable
    {
        public void Dispose() { }
    }
}
