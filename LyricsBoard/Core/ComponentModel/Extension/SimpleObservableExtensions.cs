using System;

namespace LyricsBoard.Core.ComponentModel.Extension
{
    internal static class SimpleObservableExtensions
    {
        public static IDisposable Subscribe<T>(this IObservable<T> observable, Action<T> onNext)
        {
            return observable.Subscribe(new SimpleObserver<T>(onNext));
        }
    }
}