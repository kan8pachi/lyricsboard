using BeatSaberMarkupLanguage;
using LyricsBoard.Core;
using LyricsBoard.Core.System;
using LyricsBoard.View;
using SiraUtil.Logging;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

#nullable enable

namespace LyricsBoard.View
{
    internal class BoardCanvas
    {
        private const string canvasObjectName = "LyricsBoard-BoardCanvas";
        private const string imageObjectName = "LyricsBoard-BoardImage";

        // TODO: Do we need to destroy them when Scene finished?
        private readonly GameObject canvasObject;
        private readonly GameObject imageObject;
        private readonly Canvas canvas;
        private readonly Image image;

        // standby, current, retiring
        private readonly Gen3Set<TMP_Text> tmpTexts;
        private readonly Vector3 defaultPos;
        private readonly Color defaultColor;
        private readonly float heightUnit;

        public BoardCanvas(Vector3 boardPosition, float boardWidth, float fontSize, bool showBackground)
        {
            heightUnit = fontSize * 1.8f;
            (canvasObject, canvas) = InstallCanvas(boardPosition);
            (imageObject, image) = InstallImage(canvas.transform, boardWidth, fontSize, showBackground);
            tmpTexts = InstallTextSet(image.rectTransform, fontSize);
            defaultPos = tmpTexts.Standby.transform.position;
            defaultColor = tmpTexts.Standby.color;
        }

        private (GameObject, Canvas) InstallCanvas(Vector3 position)
        {
            var go = new GameObject(canvasObjectName);
            go.layer = LayerMask.NameToLayer("UI");

            var c = go.AddComponent<Canvas>();
            c.renderMode = RenderMode.WorldSpace;

            var rotation = new Vector3(0, 0, 0);

            go.transform.position = position;
            go.transform.rotation = Quaternion.Euler(rotation);
            go.transform.localScale = Vector2.one / 10f;

            return (go, c);
        }

        private (GameObject, Image) InstallImage(Transform parent, float width, float fontSize, bool showBackground)
        {
            var go = new GameObject(imageObjectName);

            go.layer = LayerMask.NameToLayer("UI");
            var c = go.AddComponent<Image>();
            c.color = showBackground ? new Color(1f, 0, 0, 0.02f) : new Color(0, 0, 0, 0);

            var iRect = c.rectTransform;
            iRect.SetParent(parent, false);
            iRect.sizeDelta = new Vector2(width * 20f, fontSize * 4.2f);

            return (go, c);
        }

        private Gen3Set<TMP_Text> InstallTextSet(RectTransform parent, float fontSize)
        {
            var standby = InstallText(parent, fontSize);
            var current = InstallText(parent, fontSize);
            var retiring = InstallText(parent, fontSize);

            return new Gen3Set<TMP_Text>(standby, current, retiring);
        }

        private TMP_Text InstallText(RectTransform parent, float fontSize)
        {
            // hmm...? margin does not work as I expected.
            var margin = 2f;
            var anchor = new Vector2(margin, 0f);
            TMP_Text text = BeatSaberUI.CreateText(parent, "", anchor);
            text.gameObject.layer = LayerMask.NameToLayer("UI");
            text.alignment = TextAlignmentOptions.Left;
            text.overflowMode = TextOverflowModes.Ellipsis;
            text.fontSize = fontSize;
            var tRect = text.rectTransform;
            tRect.anchorMin = new Vector2(0, 0);
            tRect.anchorMax = new Vector2(1, 0);
            tRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, parent.sizeDelta.x - margin * 2);
            //tRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, fontSize);
            return text;
        }

        /// <summary>
        /// Calculate the animation curve.
        /// </summary>
        private float CalculateAnimationCurve(float progress)
        {
            // TODO: Make it smooth and super cool.
            return progress;
        }

        /// <summary>
        /// Update the displayed text, the position, and the colour(alpha) of each TextMeshPro.
        /// </summary>
        private void UpdateText(TMP_Text obj, ProgressableText ptext, float offsetY, Func<float, float> geetAlpha)
        {
            obj.text = ptext.Text;

            var animationY = CalculateAnimationCurve(ptext.Progress.GetValueOrDefault(0f));
            var posY = heightUnit * 1.1f * (offsetY + animationY + 0.6f);
            obj.transform.position = new Vector3(
                defaultPos.x,
                defaultPos.y + posY / 20f,
                defaultPos.z
            );
            obj.color = defaultColor.ColorWithAlpha(geetAlpha(animationY));
        }

        /// <summary>
        /// Update lyrics.
        /// </summary>
        public void UpdateLyricsTexts(Gen3Set<ProgressableText> animations)
        {
            UpdateText(tmpTexts.Standby, animations.Standby, 0f, x => x * 0.7f);
            UpdateText(tmpTexts.Current, animations.Current, 1f, x => 0.7f + x * 0.3f);
            UpdateText(tmpTexts.Retiring, animations.Retiring, 2f, x => 1f - x);
        }
    }

    internal class BoardViewController : IInitializable, ITickable, IDisposable
    {
        private SiraLog? logger;
        private LyricsBoardContext context;
        private GameplayCoreSceneSetupData gameplayCoreSceneSetupData;
        private AudioTimeSyncController audioTimeSyncController;

        private BoardCanvas? canvas;
        private ProgressCalculator? calculator;

        public BoardViewController(
            SiraLog? logger,
            LyricsBoardContext? context,
            GameplayCoreSceneSetupData? gameplayCoreSceneSetupData,
            AudioTimeSyncController? audioTimeSyncController
        )
        {
            this.logger = logger;

            // check if injected parameters are not null.
            if (context is null) { throw new ArgumentNullException(nameof(context)); }
            if (gameplayCoreSceneSetupData is null) { throw new ArgumentNullException(nameof(gameplayCoreSceneSetupData)); }
            if (audioTimeSyncController is null) { throw new ArgumentNullException(nameof(audioTimeSyncController)); }
            this.context = context;
            this.gameplayCoreSceneSetupData = gameplayCoreSceneSetupData;
            this.audioTimeSyncController = audioTimeSyncController;

            IsWorking = context.IsWorking && context.Config.DisplayLyricsBoard;
            IsReady = false;
        }

        private bool IsWorking { get; set; }
        private bool IsReady { get; set; }

        public void Initialize()
        {
            if (!IsWorking) { return; }

            var conf = context.Config;

            calculator = GetCalculatorForPlayingSong();
            if (calculator is null)
            {
                IsWorking = false;
                return;
            }

            var boardPosition = new Vector3(conf.BoardPositionX, conf.BoardPositionY, conf.BoardPositionZ);

            // I could not find the way to show a BSML-ed display window in the mid air, so create it by myself.
            // I would like to replace it in the future. 
            canvas = new BoardCanvas(boardPosition, conf.BoardWidth, conf.FontSize, conf.ShowBoardBackground);

            IsReady = true;
        }

        public void Dispose()
        {
            if (!IsWorking) { return; }
        }

        public void Tick()
        {
            if (!(IsWorking && IsReady)) { return; }
            if (calculator is null)
            {
                logger?.Error(nameof(calculator) + " was null. Stop working due to unexpected error.");
                IsWorking = false;
                return;
            }
            if (canvas is null)
            {
                logger?.Error(nameof(canvas) + " was null.  Stop working due to unexpected error.");
                IsWorking = false;
                return;
            }


            var time = audioTimeSyncController.songTime;
            var animations = calculator.GetPresentProgress(time);
            canvas.UpdateLyricsTexts(animations);
        }

        /// <summary>
        /// Load lyrics and return animation calculator.
        /// </summary>
        /// <returns>calculator. may be null</returns>
        private ProgressCalculator? GetCalculatorForPlayingSong()
        {
            var levelId = gameplayCoreSceneSetupData.difficultyBeatmap.level.levelID;

            // copied from https://github.com/opl-/beatsaber-http-status
            var songHash = Regex.IsMatch(
                levelId,
                "^custom_level_[0-9A-F]{40}",
                RegexOptions.IgnoreCase
            ) && !levelId.EndsWith(" WIP") ? levelId.Substring(13, 40) : null;
            if (songHash is null)
            {
                logger?.Info($"Failed to get song hash from level ID [{levelId}]. ");
            }

            var calc = context.GetLyricsProgressCalculator(songHash);
            if (calc is null)
            {
                logger?.Info($"Lyrics file not found for current song [{songHash}].");
                return null;
            }
            logger?.Debug($"Loaded lyrics for song [{songHash}].");
            return calc;
        }
    }
}
