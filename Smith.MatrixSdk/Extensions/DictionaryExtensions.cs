using System.Collections.Generic;
using System.Linq;

namespace Smith.MatrixSdk.Extensions
{
    public static class DictionaryEx
    {
        public static Dictionary<TKey, TValue> FilterNotNull<TKey, TValue>(this Dictionary<TKey, TValue?> dictionary)
            where TKey : notnull
            where TValue : class
        {
            return new(dictionary.Where(p => p.Value is not null)!);
        }
    }
}
