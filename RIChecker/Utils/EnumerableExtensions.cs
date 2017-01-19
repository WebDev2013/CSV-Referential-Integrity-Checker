using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RIChecker.Utils
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<TSource> IncrementCounter<TSource>(this IEnumerable<TSource> source, Action action)
        {
            foreach (var s in source)
            {
                action();
                yield return s;
            }
            yield break;
        }

        public static IEnumerable<TSource> SkipHeader<TSource>(this IEnumerable<TSource> source)
        {
            return source.Skip(1);
        }

        public static List<TResult> AsList<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            return source.Select(s=> selector(s)).ToList();
        }
    }
}
