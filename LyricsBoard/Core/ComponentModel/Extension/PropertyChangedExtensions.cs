using System;
using System.ComponentModel;

namespace LyricsBoard.Core.ComponentModel.Extension
{
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