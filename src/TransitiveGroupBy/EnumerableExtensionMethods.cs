using System;
using System.Collections.Generic;
using System.Linq;

namespace TransitiveGroupBy
{
    public static class EnumerableExtensionMethods
    {
        public static List<List<TSource>> TransitiveGroupBy<TSource>(
            this IEnumerable<TSource> source,
            Func<TSource, TSource, bool> relation,
            IEqualityComparer<TSource>? comparer = null)
            where TSource: notnull
        {
            var usedComparer = comparer ?? EqualityComparer<TSource>.Default;
            // Remark: We make the relation reflexiv for convenience
            // to ensure that a Value of relationDictionary (below) always contains the corresponding Key.
            var reflexiveRelation = new Func<TSource, TSource, bool>((s1, s2) => 
                usedComparer.Equals(s1, s2)
                || relation(s1, s2));
            var sourceList = source.ToList();
            var relationDictionary = sourceList
                .ToDictionary(
                    s1 => s1,
                    s1 => sourceList
                        .Where(s2 => reflexiveRelation(s1, s2))
                        .ToHashSet(comparer),
                    usedComparer);
            var sourceSetList = sourceList
                .Select(s1 => relationDictionary[s1])
                .ToList();
            var internalResult = TransitiveMergeInternal(sourceSetList, comparer);

            return internalResult
                .Select(s => s.ToList())
                .ToList();
        }

        public static List<List<TSource>> TransitiveMerge<TSource>(
            this IEnumerable<ICollection<TSource>> source,
            IEqualityComparer<TSource>? comparer = null)
            where TSource : notnull
        {
            var sourceSetList = source
                .Select(c => c.ToHashSet(comparer))
                .ToList();
            var internalResult = TransitiveMergeInternal(sourceSetList, comparer);

            return internalResult
                .Select(s => s.ToList())
                .ToList();
        }

        private static List<HashSet<TSource>> TransitiveMergeInternal<TSource>(
            List<HashSet<TSource>> sourceSetList,
            IEqualityComparer<TSource>? comparer = null)
            where TSource : notnull
        {
            var usedComparer = comparer ?? EqualityComparer<TSource>.Default;
            var indexList = new List<HashSet<TSource>>();
            var indexMapDictionary = new Dictionary<TSource, int>(usedComparer);

            foreach (var currentSet in sourceSetList)
            {
                var existingKeyList = currentSet.Intersect(indexMapDictionary.Keys).ToList();
                if (!existingKeyList.Any())
                {
                    var nextIndex = indexList.Count;
                    indexList.Add(currentSet);

                    foreach (var element in currentSet)
                    {
                        indexMapDictionary[element] = nextIndex;
                    }
                }
                else
                {
                    var index = existingKeyList.Min(element => indexMapDictionary[element]);
                    var updateList = existingKeyList
                        .Where(element => indexMapDictionary[element] != index)
                        .SelectMany(element => indexList[indexMapDictionary[element]])
                        .Union(currentSet)
                        .ToHashSet(usedComparer);
                    indexList[index].UnionWith(updateList);

                    foreach (var element in updateList)
                    {
                        indexMapDictionary[element] = index;
                    }
                }
            }

            var result = indexMapDictionary.Values
                .Distinct()
                .OrderBy(index => index)
                .Select(index => indexList[index])
                .ToList();

            if (sourceSetList.Count > result.Count)
            {
                return TransitiveMergeInternal(result, comparer);
            }

            return result;
        }
    }
}
