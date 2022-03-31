using System;
using System.Collections.Generic;
using System.Linq;

namespace LyricsBoard.Core.ComponentModel
{
    internal class SimpleDisposer : IDisposable
    {
        private List<IDisposable> disposables = new List<IDisposable>();
        private bool disposed = false;

        public void Dispose()
        {
            if (!disposed)
            {
                disposed = true;
                foreach (var d in Enumerable.Reverse(disposables))
                {
                    d?.Dispose();
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

    internal static class SimpleDisposerExtensions
    {
        public static T AddTo<T>(this T disposable, SimpleDisposer disposer) where T : IDisposable
        {
            disposer.Add(disposable);
            return disposable;
        }
    }
}
