using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.GameplaySetup;
using LyricsBoard.ComponentModel;
using LyricsBoard.Core;
using LyricsBoard.Core.ComponentModel;
using SiraUtil.Logging;
using System;
using Zenject;

namespace LyricsBoard.View
{
    internal class MenuViewController : IInitializable, IDisposable
    {
        internal class MenuHost : BindableBase, IDisposable
        {
            private readonly SiraLog logger;
            private readonly LyricsBoardContext context;
            private readonly SimpleDisposer disposer;

            private BindableProperty<bool> display;
            [UIValue("display-lyrics-board")]
            public bool DisplayLyricsBoard
            {
                get { return display.Value; }
                set { display.Value = value; }
            }

            private BindableProperty<float> positionX;
            [UIValue("board-position-x")]
            public float PositionX
            {
                get { return positionX.Value; }
                set { positionX.Value = value; }
            }

            private BindableProperty<float> positionY;
            [UIValue("board-position-y")]
            public float PositionY
            {
                get { return positionY.Value; }
                set { positionY.Value = value; }
            }

            private BindableProperty<float> positionZ;
            [UIValue("board-position-z")]
            public float PositionZ
            {
                get { return positionZ.Value; }
                set { positionZ.Value = value; }
            }

            private BindableProperty<float> boardWidth;
            [UIValue("board-width")]
            public float BoardWidth
            {
                get { return boardWidth.Value; }
                set { boardWidth.Value = value; }
            }

            private BindableProperty<float> fontSize;
            [UIValue("font-size")]
            public float FontSize
            {
                get { return fontSize.Value; }
                set { fontSize.Value = value; }
            }

            private BindableProperty<bool> additionalOffsetEnabled;
            [UIValue("additional-offset-enabled")]
            public bool AdditionalOffsetEnabled
            {
                get { return additionalOffsetEnabled.Value; }
                set { additionalOffsetEnabled.Value = value; }
            }

            private BindableProperty<int> additionalOffsetMs;
            [UIValue("additional-offset-ms")]
            public int AdditionalOffsetMs
            {
                get { return additionalOffsetMs.Value; }
                set { additionalOffsetMs.Value = value; }
            }

            private BindableProperty<bool> expirationTimeEnabled;
            [UIValue("expiration-time-enabled")]
            public bool ExpirationTimeEnabled
            {
                get { return expirationTimeEnabled.Value; }
                set { expirationTimeEnabled.Value = value; }
            }

            private BindableProperty<int> expirationTimeMs;
            [UIValue("expiration-time-ms")]
            public int ExpirationTimeMs
            {
                get { return expirationTimeMs.Value; }
                set { expirationTimeMs.Value = value; }
            }

            private BindableProperty<bool> animationDurationEnabled;
            [UIValue("animation-duration-enabled")]
            public bool AnimationDurationEnabled
            {
                get { return animationDurationEnabled.Value; }
                set { animationDurationEnabled.Value = value; }
            }

            private BindableProperty<int> animationDurationMs;
            [UIValue("animation-duration-ms")]
            public int AnimationDurationMs
            {
                get { return animationDurationMs.Value; }
                set { animationDurationMs.Value = value; }
            }

            private BindableProperty<bool> standbyDurationEnabled;
            [UIValue("standby-duration-enabled")]
            public bool StandbyDurationEnabled
            {
                get { return standbyDurationEnabled.Value; }
                set { standbyDurationEnabled.Value = value; }
            }

            private BindableProperty<int> standbyDurationMs;
            [UIValue("standby-duration-ms")]
            public int StandbyDurationMs
            {
                get { return standbyDurationMs.Value; }
                set { standbyDurationMs.Value = value; }
            }

            [UIAction("clear-cache")]
            public void ClearCache()
            {
                context.ClearSongCache();
            }

            public MenuHost(SiraLog logger, LyricsBoardContext context)
            {
                this.logger = logger;
                this.context = context;
                disposer = new SimpleDisposer();
                var conf = context.Config;

                display = conf.ToBindableProperty(
                    (c) => c.DisplayLyricsBoard,
                    (c, value) => c.DisplayLyricsBoard = value,
                    () => RaisePropertyChanged(nameof(DisplayLyricsBoard))
                ).AddTo(disposer);

                positionX = conf.ToBindableProperty(
                    (c) => c.BoardPositionX,
                    (c, value) => c.BoardPositionX = value,
                    () => RaisePropertyChanged(nameof(PositionX))
                ).AddTo(disposer);
                positionY = conf.ToBindableProperty(
                    (c) => c.BoardPositionY,
                    (c, value) => c.BoardPositionY = value,
                    () => RaisePropertyChanged(nameof(PositionY))
                ).AddTo(disposer);
                positionZ = conf.ToBindableProperty(
                    (c) => c.BoardPositionZ,
                    (c, value) => c.BoardPositionZ = value,
                    () => RaisePropertyChanged(nameof(PositionZ))
                ).AddTo(disposer);

                boardWidth = conf.ToBindableProperty(
                    (c) => c.BoardWidth,
                    (c, value) => c.BoardWidth = value,
                    () => RaisePropertyChanged(nameof(BoardWidth))
                ).AddTo(disposer);

                fontSize = conf.ToBindableProperty(
                    (c) => c.FontSize,
                    (c, value) => c.FontSize = value,
                    () => RaisePropertyChanged(nameof(FontSize))
                ).AddTo(disposer);

                //---- additional offset ----//
                additionalOffsetEnabled = conf.ToBindableProperty(
                    (c) => c.LrcAdditionalOffsetMsEnabled,
                    (c, value) => c.LrcAdditionalOffsetMsEnabled = value,
                    () => RaisePropertyChanged(nameof(AdditionalOffsetEnabled))
                ).AddTo(disposer);
                additionalOffsetMs = conf.ToBindableProperty(
                    (c) => c.LrcAdditionalOffsetMsValue,
                    (c, value) => c.LrcAdditionalOffsetMsValue = value,
                    () => RaisePropertyChanged(nameof(AdditionalOffsetMs))
                ).AddTo(disposer);

                //---- animation setting ----//
                expirationTimeEnabled = conf.ToBindableProperty(
                    (c) => c.LyricsLineMaxExpirationMsEnabled,
                    (c, value) => c.LyricsLineMaxExpirationMsEnabled = value,
                    () => RaisePropertyChanged(nameof(ExpirationTimeEnabled))
                ).AddTo(disposer);
                expirationTimeMs = conf.ToBindableProperty(
                    (c) => c.LyricsLineMaxExpirationMsValue,
                    (c, value) => c.LyricsLineMaxExpirationMsValue = value,
                    () => RaisePropertyChanged(nameof(ExpirationTimeMs))
                ).AddTo(disposer);
                animationDurationEnabled = conf.ToBindableProperty(
                    (c) => c.LyricsAnimationDurationMsEnabled,
                    (c, value) => c.LyricsAnimationDurationMsEnabled = value,
                    () => RaisePropertyChanged(nameof(AnimationDurationEnabled))
                ).AddTo(disposer);
                animationDurationMs = conf.ToBindableProperty(
                    (c) => c.LyricsAnimationDurationMsValue,
                    (c, value) => c.LyricsAnimationDurationMsValue = value,
                    () => RaisePropertyChanged(nameof(AnimationDurationMs))
                ).AddTo(disposer);
                standbyDurationEnabled = conf.ToBindableProperty(
                    (c) => c.LyricsStandbyDurationMsEnabled,
                    (c, value) => c.LyricsStandbyDurationMsEnabled = value,
                    () => RaisePropertyChanged(nameof(StandbyDurationEnabled))
                ).AddTo(disposer);
                standbyDurationMs = conf.ToBindableProperty(
                    (c) => c.LyricsStandbyDurationMsValue,
                    (c, value) => c.LyricsStandbyDurationMsValue = value,
                    () => RaisePropertyChanged(nameof(StandbyDurationMs))
                ).AddTo(disposer);
            }

            public void Dispose()
            {
                disposer.Dispose();
            }
        }

        protected const string tabName = "LyricsBoard";
        protected const string resourceMenuBsml = "LyricsBoard.View.Menu.bsml";

        protected readonly SiraLog logger;
        protected readonly LyricsBoardContext context;
        protected MenuHost? host;

        public MenuViewController(
            SiraLog logger,
            LyricsBoardContext context
        )
        {
            this.logger = logger;
            this.context = context;
        }

        public void Initialize()
        {
            host = new(logger, context);

            GameplaySetup.instance.AddTab(tabName, resourceMenuBsml, host);
        }

        public void Dispose()
        {
            if (GameplaySetup.IsSingletonAvailable)
            {
                GameplaySetup.instance.RemoveTab(tabName);
            }
            host?.Dispose();
        }
    }
}
