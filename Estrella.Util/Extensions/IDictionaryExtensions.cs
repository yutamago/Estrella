using System.Collections.Generic;

namespace Estrella.Util
{
    public static class IDictionaryExtensions
    {
        public static TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> instance, TKey key,
            TValue defaultValue = default(TValue))
        {
            return instance.TryGetValue(key, out var result) ? result : defaultValue;
        }
    }
}