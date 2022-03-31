using LyricsBoard.ComponentModel;
using LyricsBoard.Core.ComponentModel;
using System;
using System.Collections.Generic;
using UnityEngine;

#nullable enable

namespace LyricsBoard.Configuration
{
    public class PluginConfig : BindableBase
    {
        //------ additional offset ms ------//
        private int _lrcAdditionalOffsetMsValue = 0;

        /// <summary>
        /// Offset in milliseconds to be added to the original definition in file.
        /// This should be: -3600 * 1000 (-60 min) <= value <= 3600 * 1000 (60 min).
        /// </summary>
        public int LrcAdditionalOffsetMsValue {
            get { return _lrcAdditionalOffsetMsValue; }
            set { _lrcAdditionalOffsetMsValue = Mathf.Clamp(value, -3600 * 1000, 3600 * 1000); }
        }

        public bool LrcAdditionalOffsetMsEnabled { get; set; } = false;

        //------ line max expiration ms ------//
        private int _lyricsLineMaxExpirationMsValue = 10 * 1000;

        /// <summary>
        /// Expiration time of the lyrics line in milliseconds.
        /// If enabled, the value in the definition file will be overwritten.
        /// The expiration time will be shortened if there is no enough time before the next line starts.
        /// This should be: 100 <= value <= 3600 * 1000 (60 min).
        /// </summary>
        public int LyricsLineMaxExpirationMsValue {
            get { return _lyricsLineMaxExpirationMsValue; }
            set { _lyricsLineMaxExpirationMsValue = Mathf.Clamp(value, 100, 3600 * 1000); }
        }

        public bool LyricsLineMaxExpirationMsEnabled { get; set; } = false;

        private int _LyricsLineMaxExpirationDefaultMs = 7000;
        public int LyricsLineMaxExpirationDefaultMs {
            get { return _LyricsLineMaxExpirationDefaultMs; }
            set { _LyricsLineMaxExpirationDefaultMs = Mathf.Clamp(value, 100, 3600 * 1000); }
        }

        //------ animation duration ms ------//
        private int _lyricsAnimationDurationMsValue = 200;

        /// <summary>
        /// The duration time of the animation when the lines of lyrics move.
        /// If enabled, the value in the definition file will be overwritten.
        /// This should be: 0 <= value <= 60 * 1000 (1 min).
        /// </summary>
        public int LyricsAnimationDurationMsValue {
            get { return _lyricsAnimationDurationMsValue; }
            set { _lyricsAnimationDurationMsValue = Mathf.Clamp(value, 0, 60 * 1000); }
        }

        public bool LyricsAnimationDurationMsEnabled { get; set; } = false;

        private int _LyricsAnimationDurationDefaultMs = 200;
        public int LyricsAnimationDurationDefaultMs
        {
            get { return _LyricsAnimationDurationDefaultMs; }
            set { _LyricsAnimationDurationDefaultMs = Mathf.Clamp(value, 0, 60 * 1000); }
        }

        //------ standby duration ms ------//
        private int _lyriStandbyDurationMsValue = 200;

        /// <summary>
        /// The duration time that the next lyrics line lives for at the standby area.
        /// If enabled, the value in the definition file will be overwritten.
        /// This should be: 100 <= value <= 3600 * 1000 (60 min).
        /// </summary>
        public int LyricsStandbyDurationMsValue
        {
            get { return _lyriStandbyDurationMsValue; }
            set { _lyriStandbyDurationMsValue = Mathf.Clamp(value, 0, 3600 * 1000); }
        }

        public bool LyricsStandbyDurationMsEnabled { get; set; } = false;

        private int _LyricsStandbyDurationDefaultMs = 1500;
        public int LyricsStandbyDurationDefaultMs
        {
            get { return _LyricsStandbyDurationDefaultMs; }
            set { _LyricsStandbyDurationDefaultMs = Mathf.Clamp(value, 0, 3600 * 1000); }
        }

        //------ cache size ------//
        private int _cacheSize = 30;

        /// <summary>
        /// The number of objects that will be cached in memory.
        /// This should be: 1 <= CacheSize <= 300.
        /// </summary>
        public int CacheSize
        {
            get { return _cacheSize; }
            set { _cacheSize = Mathf.Clamp(value, 1, 300); }
        }

        public float BoardPositionX { get; set; } = 0f;
        public float BoardPositionY { get; set; } = -0.2f;
        public float BoardPositionZ { get; set; } = 9.0f;
        public float BoardWidth { get; set; } = 4.0f;
        public float FontSize { get; set; } = 3.0f;
        public bool DisplayLyricsBoard { get; set; } = true;
        public bool ShowBoardBackground { get; set; } = false;

        /// <summary>
        /// This is called whenever BSIPA reads the config from disk (including when file changes are detected).
        /// </summary>
        //public virtual void OnReload()
        //{
        //}
    }

    public static class PluginConfigExtensions
    {
        public static int GetLrcAdditionalOffsetMs(this PluginConfig conf, int? valueInDef)
        {
            var valueInConf = conf.LrcAdditionalOffsetMsEnabled ? conf.LrcAdditionalOffsetMsValue : (int?)null;
            return valueInConf ?? valueInDef ?? 0;
        }

        public static int GetLyricsLineMaxExpirationMs(this PluginConfig conf, int? valueInDef)
        {
            var valueInConf = conf.LyricsLineMaxExpirationMsEnabled ? conf.LyricsLineMaxExpirationMsValue : (int?)null;
            var defaultInConf = conf.LyricsLineMaxExpirationDefaultMs;
            return valueInConf ?? valueInDef ?? defaultInConf;
        }

        public static int GetLyricsAnimationDurationMs(this PluginConfig conf, int? valueInDef)
        {
            var valueInConf = conf.LyricsAnimationDurationMsEnabled ? conf.LyricsAnimationDurationMsValue : (int?)null;
            var defaultInConf = conf.LyricsAnimationDurationDefaultMs;
            return valueInConf ?? valueInDef ?? defaultInConf;
        }

        public static int GetLyricsStandbyDurationMs(this PluginConfig conf, int? valueInDef)
        {
            var valueInConf = conf.LyricsStandbyDurationMsEnabled ? conf.LyricsStandbyDurationMsValue : (int?)null;
            var defaultInConf = conf.LyricsStandbyDurationDefaultMs;
            return valueInConf ?? valueInDef ?? defaultInConf;
        }
    }
}