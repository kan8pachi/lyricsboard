using IPA.Logging;
using SiraUtil.Logging;
using System;

namespace LyricsBoard.Core.K8
{
    // implement wrapper logger to make test project independent of IPA/SiraUtils related component.
    internal class K8Logger
    {
        public Logger? UpstreamLogger { get; private set; }
        public K8Logger(Logger? upstreamLogger)
        {
            UpstreamLogger = upstreamLogger;
        }

        public void Trace(string message) => UpstreamLogger?.Trace(message);
        public void Trace(Exception e) => UpstreamLogger?.Trace(e);
        public void Debug(string message) => UpstreamLogger?.Debug(message);
        public void Debug(Exception e) => UpstreamLogger?.Debug(e);
        public void Info(string message) => UpstreamLogger?.Info(message);
        public void Info(Exception e) => UpstreamLogger?.Info(e);
        public void Warn(string message) => UpstreamLogger?.Warn(message);
        public void Warn(Exception e) => UpstreamLogger?.Warn(e);
        public void Error(string message) => UpstreamLogger?.Error(message);
        public void Error(Exception e) => UpstreamLogger?.Error(e);
        public K8Logger GetChildK8Logger(string name)
        {
            return new K8Logger(UpstreamLogger?.GetChildLogger(name));
        }
    }

    internal static class K8LoggerExtention
    {
        public static K8Logger GetChildK8Logger(this SiraLog siraLog, string name)
        {
            var upstreamChildLogger = siraLog?.Logger?.GetChildLogger(name);
            return new K8Logger(upstreamChildLogger);
        }

        public static K8Logger GetChildK8Logger(this Logger logger, string name)
        {
            var upstreamChildLogger = logger.GetChildLogger(name);
            return new K8Logger(upstreamChildLogger);
        }
    }
}
