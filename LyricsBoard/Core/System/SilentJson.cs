using Newtonsoft.Json;
using System;

namespace LyricsBoard.Core.System
{
    internal interface IJson
    {
        public T? DeserializeObjectOrDefault<T>(string value);
    }

    internal class SilentJson : IJson
    {
        public T? DeserializeObjectOrDefault<T>(string value)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(value);
            }
            catch (Exception ex) when (ex is JsonReaderException || ex is JsonSerializationException)
            {
                return default;
            }
        }
    }
}
