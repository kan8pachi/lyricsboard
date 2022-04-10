using FluentAssertions;
using LyricsBoard.ComponentModel;
using LyricsBoard.Core.ComponentModel.Extension;
using Moq;
using System;
using System.Reflection;
using Xunit;

namespace LyricsBoardTest.Core.ComponentModel
{
    public class BindablePropertyTests
    {
        internal class ModelMock : BindableBase
        {
            private int propInt = 1;
            public int PropInt
            {
                get { return propInt; }
                set { SetProperty(ref propInt, value); }
            }

            public int EventCount {
                get
                {
                    var fieldInfo = typeof(BindableBase).GetField(
                        nameof(PropertyChanged),
                        BindingFlags.Instance | BindingFlags.NonPublic
                    );
                    var eventDelegate = (MulticastDelegate)fieldInfo.GetValue(this);
                    return eventDelegate is null
                        ? 0
                        : eventDelegate.GetInvocationList().Length;
                }
            }
        }

        [Fact]
        public void BindableProperty_ModelChange()
        {
            var model = new ModelMock();
            var notifier = new Mock<Action>();
            var bp = model.ToBindableProperty(
                x => x.PropInt,
                notifier.Object
            );

            notifier.Verify(x => x(), Times.Never(), "notifier should not be called before changing the property of model.");
            bp.Value.Should().Be(1);

            // Action!!
            model.PropInt = 2;

            notifier.Verify(x => x(), Times.Once());
            bp.Value.Should().Be(2);
        }

        [Fact]
        public void BindableProperty_ValueChange()
        {
            var model = new ModelMock();
            var notifier = new Mock<Action>();
            var bp = model.ToBindableProperty(
                x => x.PropInt,
                notifier.Object
            );

            model.PropInt.Should().Be(1);

            // Action!!
            bp.Value = 2;

            notifier.Verify(x => x(), Times.Never(), "notifier should not be called on value change.");
            model.PropInt.Should().Be(2);
        }

        [Fact]
        public void BindableProperty_Dispose()
        {
            var model = new ModelMock();
            var notifier = new Mock<Action>();
            {
                var bp = model.ToBindableProperty(
                    x => x.PropInt,
                    notifier.Object
                );

                model.EventCount.Should().Be(1, "event handler should be added");

                bp.Dispose();
            }
            model.EventCount.Should().Be(0, "event handler should be removed by Dispose()");
        }
    }
}
