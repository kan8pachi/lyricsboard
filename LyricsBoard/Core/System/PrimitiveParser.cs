namespace LyricsBoard.Core.System
{
    internal static class PrimitiveParser
    {
        public static long? ParseLongOrDefault(string? text, long? defaultValue)
        {
            return long.TryParse(text, out var result) ? result : defaultValue;
        }

        public static int? ParseIntOrDefault(string? text, int? defaultValue)
        {
            return int.TryParse(text, out var result) ? result : defaultValue;
        }
    }
}
