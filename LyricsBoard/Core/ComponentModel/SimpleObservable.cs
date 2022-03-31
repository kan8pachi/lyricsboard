using System;

namespace LyricsBoard.Core.ComponentModel
{
    internal class SimpleObserver<T> : IObserver<T>
    {
        protected readonly Action<T> onNext;
        public SimpleObserver(Action<T> onNext) => this.onNext = onNext;
        public void OnNext(T value) => onNext(value);
        public void OnCompleted() { }
        public void OnError(Exception error) { }
    }

    internal class SimpleObservable<T> : IObservable<T>
    {
        protected readonly Func<IObserver<T>, IDisposable> subscribe;
        public SimpleObservable(Func<IObserver<T>, IDisposable> subscribe) => this.subscribe = subscribe;
        public IDisposable Subscribe(IObserver<T> observer) => subscribe(observer);
    }
}
