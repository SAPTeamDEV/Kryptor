namespace SAPTeam.Kryptor.Extensions
{
    /// <summary>
    /// Represents extension methods for <see cref="IEnumerable{T}"/> collections.
    /// </summary>
    public static class EnumerableExtensions
    {
#if !NET6_0_OR_GREATER
        /// <summary>
        /// Split the elements of a sequence into chunks of size at most <paramref name="size"/>.
        /// </summary>
        /// <remarks>
        /// Every chunk except the last will be of size <paramref name="size"/>.
        /// The last chunk will contain the remaining elements and may be of a smaller size.
        /// </remarks>
        /// <param name="source">
        /// An <see cref="IEnumerable{T}"/> whose elements to chunk.
        /// </param>
        /// <param name="size">
        /// Maximum size of each chunk.
        /// </param>
        /// <typeparam name="TSource">
        /// The type of the elements of source.
        /// </typeparam>
        /// <returns>
        /// An <see cref="IEnumerable{T}"/> that contains the elements the input sequence split into chunks of size <paramref name="size"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source"/> is null.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="size"/> is below 1.
        /// </exception>
        public static IEnumerable<TSource[]> Chunk<TSource>(this IEnumerable<TSource> source, int size)
        {
            return source == null
                ? throw new ArgumentNullException("source")
                : size < 1 ? throw new ArgumentOutOfRangeException("size") : ChunkIterator(source, size);
        }
#endif

        /// <summary>
        /// Finds given pattern in a byte array.
        /// </summary>
        /// <param name="src">
        /// Source byte array.
        /// </param>
        /// <param name="pattern">
        /// The pattern to be searched.
        /// </param>
        /// <returns>Start index of first occurrence.</returns>
        public static int LocatePattern<T>(this IEnumerable<T> src, IEnumerable<T> pattern)
        {
            for (int i = 0; i < src.Count(); i++)
            {
                if (src.Skip(i).Take(pattern.Count()).SequenceEqual(pattern))
                {
                    return i;
                }
            }

            return -1;
        }

        private static IEnumerable<TSource[]> ChunkIterator<TSource>(IEnumerable<TSource> source, int size)
        {
            using (IEnumerator<TSource> e = source.GetEnumerator())
            {
                while (e.MoveNext())
                {
                    TSource[] chunk = new TSource[size];
                    chunk[0] = e.Current;

                    int i = 1;
                    for (; i < chunk.Length && e.MoveNext(); i++)
                    {
                        chunk[i] = e.Current;
                    }

                    if (i == chunk.Length)
                    {
                        yield return chunk;
                    }
                    else
                    {
                        Array.Resize(ref chunk, i);
                        yield return chunk;
                        yield break;
                    }
                }
            }
        }
    }
}
