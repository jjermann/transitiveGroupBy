# TransitiveGroupBy
This is a simple mini project to provide two extension methods to group collections by a transitively completed relation.

# Motivation
The already existing `GroupBy` assumes / only properly works with equivalence relations (i.e. reflexive, symmetric, transitive relations).

A more general (in particular non-transitive) relation can be extended to an equivalence relation by simply (transitively) accumulating all related entries into one group.
This can/could be very useful in some situations.

This very implicit GroupBy cannot easily be deduced from existing methods like `GroupBy`. Hence this mini project.

# Documentation
```csharp
public static List<List<TSource>> TransitiveGroupBy<TSource>(
    this IEnumerable<TSource> source,
    Func<TSource, TSource, bool> relation,
    IEqualityComparer<TSource>? comparer = null)
    where TSource: notnull
```
`source.TransitiveGroupBy(relation)` groups the given `source` (into a list of lists) based on the transitively completed relation.

I.e. two elements are in the same group if and only if there exists a sequence `s_{1}, s_{2}, ..., s_{n}` with `relation(s_{i}, s_{i+1}) = true` for all `i=1,...,n-1`.

The algorithm will internally use the provided `comparer` to distinguish elements.
If `comparer = null` (most common case) then the default EqualityComparer for `TSource` is used.

Remark: It doesn't matter if the relation is reflexive or symmetric (the result will be the same if the relation is made reflexive/symmetric).

```csharp
public static List<List<TSource>> TransitiveMerge<TSource>(
    this IEnumerable<ICollection<TSource>> source,
    IEqualityComparer<TSource>? comparer = null)
    where TSource : notnull
```
`TransitiveMerge(sourceCollectionEnumerable, comparer)` merges intersecting lists until they are distinct.

The algorithm will internally use the provided `comparer` to distinguish elements.
If `comparer = null` then the default EqualityComparer for `TSource` is used.
Equal elements will be reduced to one element.

# Examples
TODO