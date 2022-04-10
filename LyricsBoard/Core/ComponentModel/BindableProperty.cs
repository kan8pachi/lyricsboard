using LyricsBoard.Core.ComponentModel.Extension;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace LyricsBoard.Core.ComponentModel
{
    internal class BindableProperty<TProperty> : IDisposable
    {
        private readonly SimpleDisposer disposer = new SimpleDisposer();
        private readonly Func<TProperty> sourceGetter;
        private readonly Action<TProperty> sourceSetter;
        private readonly Action notifier;

        public BindableProperty(
            INotifyPropertyChanged subject,
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
            subject
                .PropertyChangedAsObservable()
                .Subscribe(OnSourceChanged)
                .AddTo(disposer);
        }

        public void Dispose() => disposer.Dispose();

        private TProperty _value;
        public TProperty Value
        {
            get { return _value; }
            set {
                if (!EqualityComparer<TProperty>.Default.Equals(_value, value))
                {
                    _value = value;
                    sourceSetter(value);
                }
            }
        }

        private void OnSourceChanged(PropertyChangedEventArgs args)
        {
            var newValue = sourceGetter.Invoke();
            if (!EqualityComparer<TProperty>.Default.Equals(_value, newValue))
            {
                Value = newValue;
                notifier?.Invoke();
            }
        }
    }
}
