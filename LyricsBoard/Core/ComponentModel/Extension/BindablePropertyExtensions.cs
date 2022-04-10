using LyricsBoard.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LyricsBoard.Core.ComponentModel.Extension
{
    internal static class BindablePropertyExtensions
    {
        public static BindableProperty<TProperty> ToBindableProperty<TSubject, TProperty>(
            this TSubject bindTarget,
            Func<TSubject, TProperty> sourceGetter,
            Action<TSubject, TProperty> sourceSetter,
            Action notifier
        )
            where TSubject : INotifyPropertyChanged
        {
            return new BindableProperty<TProperty>(
                bindTarget,
                () => sourceGetter.Invoke(bindTarget),
                (value) => sourceSetter?.Invoke(bindTarget, value),
                notifier,
                sourceGetter.Invoke(bindTarget)
            );
        }

        public static BindableProperty<TProperty> ToBindableProperty<TSubject, TProperty>(
            this TSubject bindTarget,
            Expression<Func<TSubject, TProperty>> propertySelector,
            Action notifier
        )
            where TSubject : INotifyPropertyChanged
        {
            var getter = propertySelector.Compile();
            var setter = CreateSetAccessor(propertySelector);

            return bindTarget.ToBindableProperty(getter, setter, notifier);
        }

        public static Action<TSubject, TProperty> CreateSetAccessor<TSubject, TProperty>(
            Expression<Func<TSubject, TProperty>> propertySelector
        )
        {
            var propertyInfo = (PropertyInfo)((MemberExpression)propertySelector.Body).Member;
            var paramTSubject = Expression.Parameter(typeof(TSubject), "subject");
            var paramTProperty = Expression.Parameter(typeof(TProperty), "value");
            var body = Expression.Assign(Expression.Property(paramTSubject, propertyInfo), paramTProperty);
            var lambda = Expression.Lambda<Action<TSubject, TProperty>>(body, paramTSubject, paramTProperty);
            return lambda.Compile();
        }
    }
}
