using System;
using System.Collections.Generic;
using System.ComponentModel;

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

    internal static class SimpleObservableExtensions
    {
        public static IDisposable Subscribe<T>(this IObservable<T> observable, Action<T> onNext)
        {
            return observable.Subscribe(new SimpleObserver<T>(onNext));
        }
    }

    internal static class PropertyChangedExtensions
    {
        private static IObservable<TEventArgs> FromEvent<TEventHandler, TEventArgs>(
            Func<Action<TEventArgs>, TEventHandler> convert,
            Action<TEventHandler> addHandler,
            Action<TEventHandler> removeHandler
        )
        {
            return new SimpleObservable<TEventArgs>(observer =>
            {
                void handler(TEventArgs args) { observer.OnNext(args); }
                var h = convert(handler);
                addHandler(h);
                return new SimpleDisposable(() => removeHandler(h));
            });
        }

        public static IObservable<PropertyChangedEventArgs> PropertyChangedAsObservable<T>(this T subject)
            where T : INotifyPropertyChanged
        {
            return FromEvent<PropertyChangedEventHandler, PropertyChangedEventArgs>(
                h => (sender, e) => h(e),
                h => subject.PropertyChanged += h,
                h => subject.PropertyChanged -= h
            );
        }
    }
}
