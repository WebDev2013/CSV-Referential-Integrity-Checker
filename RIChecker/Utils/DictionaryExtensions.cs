using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Ref: http://stackoverflow.com/questions/2601477/dictionary-returning-a-default-value-if-the-key-does-not-exist

namespace RIChecker.Utils
{
    public static class DictionaryExtensions
    {
        public static TValue GetValueOrDefault<TKey, TValue> (this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue = default(TValue))
        {
            TValue value;
            return dictionary.TryGetValue(key, out value) ? value : defaultValue;
        }

        public static TValue GetValueOrDefault<TKey, TValue> (this IDictionary<TKey, TValue> dictionary, TKey key, Func<TValue> defaultValueProvider)
        {
            TValue value;
            return dictionary.TryGetValue(key, out value) ? value : defaultValueProvider();
        }

        //public static List<TValue> GetValues<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey[] keys)
        //{
        //    return keys
        //        .Where(x => dictionary.ContainsKey(x))
        //        .Select(x => dictionary[x])
        //        .ToList();
        //}

        public static List<TValue> GetValues<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, params TKey[] keys)
        {
            return keys
                .Where(x => dictionary.ContainsKey(x))
                .Select(x => dictionary[x])
                .ToList();
        }
    }
}
