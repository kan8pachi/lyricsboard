using IPA.Logging;
using SiraUtil.Logging;

namespace LyricsBoard.Core.Logging.Extension
{
    internal static class K8LoggerExtensions
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
