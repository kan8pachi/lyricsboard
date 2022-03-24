using System;

namespace LyricsBoard.Core.ComponentModel
{
    internal class BindableProperty<TProperty> : IDisposable
    {
        private SimpleDisposer disposer = new SimpleDisposer();
        private Func<TProperty> sourceGetter;
        private Action<TProperty> sourceSetter;
        private Action notifier;

        public BindableProperty(
            ISimpleObservable observable,
            Func<TProperty> sourceGetter,
            Action<TProperty> sourceSetter,
            Action notifier,
            TProperty initialValue
        )
        {
            this.sourceGetter = sourceGetter;
            this.sourceSetter = sourceSetter;
            this.notifier = notifier;

            _value = initialValue;
            observable
                .Subscribe(OnSourceChanged)
                .AddTo(disposer);
        }

        public void Dispose() => disposer.Dispose();

        private TProperty _value;
        public TProperty Value
        {
            get { return _value; }
            set {
                if (!_value.Equals(value))
                {
                    _value = value;
                    sourceSetter?.Invoke(value);
                }
            }
        }

        private void OnSourceChanged()
        {
            var newValue = sourceGetter.Invoke();
            if (!newValue.Equals(Value))
            {
                Value = newValue;
                notifier?.Invoke();
            }
        }
    }

    internal static class StrongBindableExtensions
    {
        public static BindableProperty<TProperty> ToBindableProperty<TSubject, TProperty>(
            this TSubject observable,
            Func<TSubject, TProperty> sourceGetter,
            Action<TSubject, TProperty> sourceSetter,
            Action notifier
        )
            where TSubject : ISimpleObservable
        {
            return new BindableProperty<TProperty>(
                observable,
                () => sourceGetter.Invoke(observable),
                (value) => sourceSetter?.Invoke(observable, value),
                notifier,
                sourceGetter.Invoke(observable)
            );
        }

    }
}
