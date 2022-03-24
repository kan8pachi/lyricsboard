using System;
using System.Collections.Generic;

namespace LyricsBoard.Core.ComponentModel
{
    public interface ISimpleObserver
    {
        void OnNext();
    }

    public interface ISimpleObservable
    {
        IDisposable Subscribe(ISimpleObserver observer);
    }

    internal class SimpleDisposable : IDisposable
    {
        private readonly Action disposeAction;
        public SimpleDisposable(Action disposeAction) => this.disposeAction = disposeAction;

        public void Dispose() => disposeAction?.Invoke();
    }

    public class SimpleObservableTrait : ISimpleObservable
    {
        List<ISimpleObserver> observers = new List<ISimpleObserver>();

        public IDisposable Subscribe(ISimpleObserver observer)
        {
            if (observer == null)
            {
                throw new ArgumentNullException(nameof(observer));
            }

            observers.Add(observer);

            return new SimpleDisposable(() =>
            {
                if (observers.Contains(observer))
                {
                    observers.Remove(observer);
                }
            });
        }

        protected void InvokeOnNext()
        {
            foreach (var observer in observers)
            {
                observer.OnNext();
            }
        }
    }

    public class SimpleObserver : ISimpleObserver
    {
        protected readonly Action onNext;
        public SimpleObserver(Action onNext) => this.onNext = onNext;

        public void OnNext() => onNext?.Invoke();
    }

    public static class SimpleObservableExtensions
    {
        public static IDisposable Subscribe(this ISimpleObservable observable, Action onNext)
        {
            return observable.Subscribe(new SimpleObserver(onNext));
        }
    }
}
