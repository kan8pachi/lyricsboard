using System;
using System.Collections.Generic;

namespace LyricsBoard.Core.ComponentModel
{
    public class SimpleDisposer : IDisposable
    {
        private List<IDisposable> disposables = new List<IDisposable>();
        private bool disposed = false;
        public void Dispose()
        {
            if (!disposed)
            {
                disposed = true;
                foreach (var disposable in disposables)
                {
                    disposable?.Dispose();
                }
            }
        }
        public void Add(IDisposable item)
        {
            if (disposed)
            {
                item.Dispose();
            }
            disposables.Add(item);
        }
    }

    public static class SimpleDisposerExtensions
    {
        public static T AddTo<T>(this T disposable, SimpleDisposer disposer) where T : IDisposable
        {
            disposer.Add(disposable);
            return disposable;
        }
    }
}
