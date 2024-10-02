namespace SAPTeam.Kryptor.Helpers
{
    /// <summary>
    /// Provides methods to use in async-challenging situations.
    /// </summary>
    /// <remarks>
    /// Currently tested with android.
    /// </remarks>
    public static class AsyncCompat
    {
        /// <summary>
        /// Gets a value indicating that the current environment has the async compatibility.
        /// </summary>
        public static bool IsAsyncCompatible { get; set; }

        static AsyncCompat() => IsAsyncCompatible = true;

        /// <summary>
        /// Suspends the current thread or creates a task that completes after a specified number of milliseconds.
        /// </summary>
        /// <param name="ms">
        /// Delay time in millisecconds.
        /// </param>
        public static async Task Delay(int ms) => await Delay(ms, CancellationToken.None);

        /// <summary>
        /// Suspends the current thread or creates a cancellable task that completes after a specified number of milliseconds.
        /// </summary>
        /// <param name="ms">
        /// Delay time in millisecconds.
        /// </param>
        /// <param name="cancellationToken">
        /// A cancellation token to observe while waiting for the task to complete. (Only in async mode)
        /// </param>
        public static async Task Delay(int ms, CancellationToken cancellationToken)
        {
            if (IsAsyncCompatible)
            {
                await Task.Delay(ms, cancellationToken);
            }
            else
            {
                Thread.Sleep(ms);
            }
        }

        /// <summary>
        /// Writes a sequence of bytes to the stream and advances the current position within the stream by the number of bytes written.
        /// </summary>
        /// <param name="stream">
        /// The source stream with write ability.
        /// </param>
        /// <param name="buffer">
        /// An array of bytes.
        /// </param>
        /// <param name="offset">
        /// The zero-based byte offset in buffer at which to begin copying bytes to the stream.
        /// </param>
        /// <param name="count">
        /// The number of bytes to be written to the stream.
        /// </param>
        public static async Task WriteAsync(Stream stream, byte[] buffer, int offset, int count) => await WriteAsync(stream, buffer, offset, count, CancellationToken.None);

        /// <summary>
        /// Writes a sequence of bytes to the stream and advances the current position within the stream by the number of bytes written.
        /// </summary>
        /// <param name="stream">
        /// The source stream with write ability.
        /// </param>
        /// <param name="buffer">
        /// An array of bytes.
        /// </param>
        /// <param name="offset">
        /// The zero-based byte offset in buffer at which to begin copying bytes to the stream.
        /// </param>
        /// <param name="count">
        /// The number of bytes to be written to the stream.
        /// </param>
        /// <param name="cancellationToken">
        /// A cancellation token to observe while waiting for the task to complete. (Only in async mode)
        /// </param>
        public static async Task WriteAsync(Stream stream, byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (IsAsyncCompatible)
            {
                await stream.WriteAsync(buffer, offset, count, cancellationToken);
            }
            else
            {
                stream.Write(buffer, offset, count);
            }
        }

        /// <summary>
        /// Reads a sequence of bytes from the stream and advances the current position within the stream by the number of bytes read.
        /// </summary>
        /// <param name="stream">
        /// The source stream with read ability.
        /// </param>
        /// <param name="buffer">
        /// An array of bytes.
        /// </param>
        /// <param name="offset">
        /// The zero-based byte offset in buffer at which to begin storing the data read from the stream.
        /// </param>
        /// <param name="count">
        /// The maximum number of bytes to be read from the current stream.
        /// </param>
        public static async Task ReadAsync(Stream stream, byte[] buffer, int offset, int count) => await ReadAsync(stream, buffer, offset, count, CancellationToken.None);

        /// <summary>
        /// Reads a sequence of bytes from the stream and advances the current position within the stream by the number of bytes read.
        /// </summary>
        /// <param name="stream">
        /// The source stream with read ability.
        /// </param>
        /// <param name="buffer">
        /// An array of bytes.
        /// </param>
        /// <param name="offset">
        /// The zero-based byte offset in buffer at which to begin storing the data read from the stream.
        /// </param>
        /// <param name="count">
        /// The maximum number of bytes to be read from the current stream.
        /// </param>
        /// <param name="cancellationToken">
        /// A cancellation token to observe while waiting for the task to complete. (Only in async mode)
        /// </param>
        public static async Task ReadAsync(Stream stream, byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (IsAsyncCompatible)
            {
                await stream.ReadAsync(buffer, offset, count, cancellationToken);
            }
            else
            {
                stream.Read(buffer, offset, count);
            }
        }

        /// <summary>
        /// Reads a line of characters from the text reader and returns the data as a string.
        /// </summary>
        /// <param name="textReader">
        /// The source text reader.
        /// </param>
        /// <returns>
        /// The next line from the reader, or <see langword="null"/> if all characters have been read.
        /// </returns>
        public static async Task<string> ReadLineAsync(TextReader textReader) => IsAsyncCompatible ? await textReader.ReadLineAsync() : textReader.ReadLine();
    }
}