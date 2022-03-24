//using System;

//namespace LyricsBoard.Core.ComponentModel
//{
//    internal class WeakSubscription<T> : IDisposable, IObserver<T>
//    {
//        private readonly WeakReference<IObserver<T>> weakObserver;
//        private readonly IDisposable subscription;

//        public WeakSubscription(IObservable<T> observable, IObserver<T> observer)
//        {
//            weakObserver = new WeakReference<IObserver<T>>(observer);
//            subscription = observable.Subscribe(this);
//        }

//        bool isDisposed = false;
//        public void Dispose()
//        {
//            if (!isDisposed)
//            {
//                isDisposed = true;
//                subscription.Dispose();
//            }
//        }

//        private static void WeakAction<K>(WeakReference<K> weak, Action<K> action, Action onFail) where K : class
//        {
//            if (weak.TryGetTarget(out var reference))
//            {
//                action.Invoke(reference);
//            }
//            else
//            {
//                onFail?.Invoke();
//            }
//        }

//        public void OnCompleted()
//        {
//            WeakAction(weakObserver, (o) => o.OnCompleted(), () => Dispose());
//        }

//        public void OnError(Exception error)
//        {
//            WeakAction(weakObserver, (o) => o.OnError(error), () => Dispose());
//        }

//        public void OnNext(T value)
//        {
//            WeakAction(weakObserver, (o) => o.OnNext(value), () => Dispose());
//        }
//    }
//}
