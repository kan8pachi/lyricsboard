using SiraUtil.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;

namespace LyricsBoard.Core.System
{
    internal interface IFileSystem
    {
        /// <summary>
        /// Make sure that the specified directory is present. Directory will be created recursively when not exist.
        /// </summary>
        /// <param name="path">path to the directory</param>
        /// <returns>true when created or already exists, otherwise false.</returns>
        bool EnsureDirectoryIsPresent(string path);

        string ReadTextAllOrEmpty(string path);

        List<string>? ReatTextLinesOrNull(string path);
    }

    internal class SilentFileSystem : IFileSystem
    {
        SiraLog? logger;

        public SilentFileSystem(SiraLog? logger)
        {
            this.logger = logger;
        }

        public bool EnsureDirectoryIsPresent(string path)
        {
            if (Directory.Exists(path))
            {
                return true;
            }

            try
            {
                // note that creating directory is an asynchronous operation.
                Directory.CreateDirectory(path);
            }
            catch (Exception ex) when (
                ex is IOException ||
                ex is UnauthorizedAccessException ||
                ex is ArgumentException ||
                ex is ArgumentNullException ||
                ex is PathTooLongException ||
                ex is DirectoryNotFoundException ||
                ex is NotSupportedException
            )
            {
                logger?.Warn($"Failed to create directory ({path}) due to unexpected reason: " + ex.ToString());
                return false;
            }

            return true;
        }

        private T? ReadTextOrDefault<T>(string path, Func<string, T> readFunc) where T : class
        {
            if (!File.Exists(path))
            {
                return default;
            }
            try
            {
                return readFunc.Invoke(path);
            }
            catch (Exception ex) when (
                ex is ArgumentException ||
                ex is ArgumentNullException ||
                ex is PathTooLongException ||
                ex is DirectoryNotFoundException ||
                ex is IOException ||
                ex is UnauthorizedAccessException ||
                ex is FileNotFoundException ||
                ex is NotSupportedException ||
                ex is SecurityException
            )
            {
                logger?.Warn($"Failed to read file ({path}) due to unexpected reason: " + ex.ToString());
                return default;
            }
        }

        public string ReadTextAllOrEmpty(string path)
        {
            return ReadTextOrDefault(
                path,
                x => File.ReadAllText(x)
            ) ?? string.Empty;
        }

        public List<string>? ReatTextLinesOrNull(string path)
        {
            return ReadTextOrDefault(
                path,
                x => File.ReadAllLines(x).ToList()
            );
        }
    }
}
