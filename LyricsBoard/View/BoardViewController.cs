using BeatSaberMarkupLanguage;
using LyricsBoard.Core;
using LyricsBoard.Core.Logging;
using LyricsBoard.Core.Logging.Extension;
using SiraUtil.Logging;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

#nullable enable

namespace LyricsBoard.View
{
    internal class TextComponent
    {
        private record LyricsCharElement(
            int Index,
            char Character,
            long TimeMs
        );

        private record FrameCache(
            LyricsCharElement[] CharElements,
            TMP_CharacterInfo[] ValidCharacterInfo,
            int CharacterCount,
            Vector3[][] Vertices
        ); 

        private K8Logger? logger;
        private TMP_Text tmptext;
        private FrameCache? frameCache;
        private readonly bool charAnimation;
        private readonly float offsetY;
        private readonly Func<float, float> funcAlpha;
        private readonly Vector3 defaultPos;
        private readonly Color defaultColor;
        private readonly float heightUnit;

        public TextComponent(K8Logger? logger, RectTransform parent, float fontSize, bool charAnimation, float offsetY, Func<float, float> funcAlpha)
        {
            this.logger = logger;

            this.charAnimation = charAnimation;
            this.offsetY = offsetY;
            this.funcAlpha = funcAlpha;
            tmptext = InstallText(parent, fontSize);

            heightUnit = fontSize * 1.8f;
            defaultPos = tmptext.transform.position;
            defaultColor = tmptext.color;
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

        private float CalculateCharActivateCurve(float progress)
        {
            return progress * (progress - 1f) * -3f;
        }

        /// <summary>
        /// Calculate the animation curve.
        /// </summary>
        private float CalculateLineFadeInCurve(float progress)
        {
            // TODO: Make it smooth and super cool.
            return progress;
        }

        /// <summary>
        /// Update the displayed text, the position, and the colour(alpha) of each TextMeshPro.
        /// </summary>
        public void UpdateText(ProgressiveData<LyricsLine>? pline, float time)
        {
            var ctext = pline is null ? string.Empty : pline.Data.PlainText;
            var isRefreshFrame = tmptext.text != ctext;
            tmptext.text = ctext;

            var lineProgress = pline is null ? 0f : pline.Progress;

            var lineAnimationY = CalculateLineFadeInCurve(lineProgress);
            var linePosY = heightUnit * 1.1f * (offsetY + lineAnimationY + 0.6f);
            tmptext.transform.position = new Vector3(
                defaultPos.x,
                defaultPos.y + linePosY / 20f,
                defaultPos.z
            );
            tmptext.color = defaultColor.ColorWithAlpha(funcAlpha(lineAnimationY));

            if (charAnimation) {
                if (pline is null) { return; }

                tmptext.ForceMeshUpdate();
                if (isRefreshFrame)
                {
                    var _validCharacterInfo = tmptext.textInfo.characterInfo.Where(x => x.isVisible).ToArray();
                    var _cCharacterCount = tmptext.textInfo.characterCount;
                    var _cCharacterInfoCount = _validCharacterInfo.Count();
                    var _cLyricsCount = ctext.Length;
                    int ccount = Math.Min(Math.Min(_cCharacterCount, _cCharacterInfoCount), _cLyricsCount);
                    if (ccount != _cCharacterCount || ccount != _cCharacterInfoCount || ccount != _cLyricsCount)
                    {
                        var _cichars = string.Join(",", _validCharacterInfo.Select(x => "" + x.character + x.index));
                        var msg = "unexpected count: (min, _cCharacterCount, _cCharacterInfoCount, _cLyricsCount) = ";
                        msg += $"({ccount}, {_cCharacterCount}, {_cCharacterInfoCount}, {_cLyricsCount}) at line [{ctext}]";
                        msg += $" whilst characterInfo is [{_cichars}]";
                        logger?.Error(msg);
                    }

                    var lyricsCache = pline.Data.Texts.SelectMany((x, index) =>
                        x.Text.ToCharArray().Select(
                            c => new LyricsCharElement(index, c, x.TimeMs)
                        )
                    ).ToArray();

                    var materialIndexMax = _validCharacterInfo.Max(x => x.materialReferenceIndex);
                    var vertexCaches = new Vector3[materialIndexMax][];
                    for (int i = 0; i < materialIndexMax; i++)
                    {
                        vertexCaches[i] = tmptext.textInfo.meshInfo[i].vertices.ToArray();
                    }

                    frameCache = new FrameCache(
                        lyricsCache,
                        _validCharacterInfo,
                        ccount,
                        vertexCaches
                    );
                }

                if (frameCache is null)
                {
                    return;
                }
                for (int i = 0; i < frameCache.CharacterCount; i++)
                {
                    var _characterInfo = frameCache.ValidCharacterInfo.ElementAt(i);

                    // calculate progress ratio for each chars.
                    var cacheIndex = _characterInfo.index;
                    var taggedTime = frameCache.CharElements.ElementAtOrDefault(cacheIndex)?.TimeMs ?? -1000;
                    var elapsed = (taggedTime / 1000f) - time;
                    var animationDuration = 0.15f;
                    var charProgress = elapsed >= 0f ? 1f : elapsed <= -animationDuration ? 0f : elapsed / -animationDuration;
                    var charAnimationY = CalculateCharActivateCurve(charProgress);

                    // apply animation for that progress.
                    var _materialIndex = _characterInfo.materialReferenceIndex;
                    var _vertexIndex = _characterInfo.vertexIndex;
                    var _vertices = tmptext.textInfo.meshInfo[_materialIndex].vertices;
                    // IndexOutOfRangeException: Index was outside the bounds of the array.
                    //var _origVertices = frameCache.Vertices[_materialIndex];
                    if (_vertices.Length > _vertexIndex + 3)
                    {
                        for (int j = _vertexIndex; j < _vertexIndex + 4; j++)
                        {
                            _vertices[j].y = _vertices[j].y + charAnimationY;
                        }
                    }
                }
                tmptext.UpdateVertexData();
            }
        }
    }

    internal class BoardCanvas
    {
        private const string canvasObjectName = "LyricsBoard-BoardCanvas";
        private const string imageObjectName = "LyricsBoard-BoardImage";

        K8Logger? logger;
        // TODO: Do we need to destroy them when Scene finished?
        private readonly GameObject canvasObject;
        private readonly GameObject imageObject;
        private readonly Canvas canvas;
        private readonly Image image;

        // standby, current, retiring
        private readonly Gen3Set<TextComponent> textComponents;

        public BoardCanvas(K8Logger? logger, Vector3 boardPosition, float boardWidth, float fontSize, bool showBackground)
        {
            this.logger = logger;

            (canvasObject, canvas) = InstallCanvas(boardPosition);
            (imageObject, image) = InstallImage(canvas.transform, boardWidth, fontSize, showBackground);
            textComponents = InstallTextSet(image.rectTransform, fontSize);
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

        private Gen3Set<TextComponent> InstallTextSet(RectTransform parent, float fontSize)
        {
            var childLogger = logger?.GetChildK8Logger(nameof(TextComponent));
            var standby = new TextComponent(childLogger, parent, fontSize, false, 0f, x => x * 0.7f);
            var current = new TextComponent(childLogger, parent, fontSize, true, 1f, x => 0.7f + x * 0.3f);
            var retiring = new TextComponent(childLogger, parent, fontSize, false, 2f, x => 1f - x);

            return new Gen3Set<TextComponent>(standby, current, retiring);
        }

        /// <summary>
        /// Update lyrics.
        /// </summary>
        public void UpdateLyricsTexts(Gen3Set<ProgressiveData<LyricsLine>?> animations, float time)
        {
            textComponents.Standby.UpdateText(animations.Standby, time);
            textComponents.Current.UpdateText(animations.Current, time);
            textComponents.Retiring.UpdateText(animations.Retiring, time);
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

            var boardPosition = new Vector3(conf.BoardPositionX, conf.BoardPositionY, conf.BoardPositionZ);

            // I could not find the way to show a BSML-ed display window in the mid air, so create it by myself.
            // I would like to replace it in the future. 
            canvas = new BoardCanvas(
                logger?.GetChildK8Logger(nameof(BoardCanvas)),
                boardPosition,
                conf.BoardWidth,
                conf.FontSize,
                conf.ShowBoardBackground
            );

            GetCalculatorForPlayingSongAsync().ContinueWith(t =>
            {
                // set calculator after async method returns the result.
                var _calc = t.Result;
                if (_calc is null)
                {
                    return;
                }
                calculator = _calc;
                IsReady = true;
            });
        }

        public void Dispose()
        {
            if (!IsReady) { return; }
        }

        public void Tick()
        {
            if (!(IsWorking && IsReady)) { return; }
            if (calculator is null)
            {
                logger?.Error(nameof(calculator) + " was null. Stop working due to unexpected error.");
                IsReady = false;
                return;
            }
            if (canvas is null)
            {
                logger?.Error(nameof(canvas) + " was null.  Stop working due to unexpected error.");
                IsReady = false;
                return;
            }


            var time = audioTimeSyncController.songTime;
            var animations = calculator.GetPresentProgress(time);
            canvas.UpdateLyricsTexts(animations, time);
        }

        /// <summary>
        /// Load lyrics and return animation calculator.
        /// </summary>
        /// <returns>calculator. may be null</returns>
        private async Task<ProgressCalculator?> GetCalculatorForPlayingSongAsync()
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

            var calc = await context.GetLyricsProgressCalculatorAsync(songHash);
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
