namespace LyricsBoard.Core.System
{
    internal static class MathN
    {
        public static int? ClampNullable(int? value, int min, int max)
        {
            if (value.HasValue)
            {
                // Do not use Mathf.Clamp() here because the test project doesn't have dependency to UnityEngine.
                var val = value.Value;
                return val < min ? min : val > max ? max : val;
            }
            return null;
        }

        public static float Clamp(float val, float min, float max)
        {
            return val < min ? min : val > max ? max : val;
        }
    }
}
