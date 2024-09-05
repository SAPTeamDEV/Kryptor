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
        /// Gets a value indicating that the current environment has the async safety.
        /// </summary>
        public static bool IsAsyncCompatible { get; }

        static AsyncCompat()
        {
            bool isAndroid = false;
#if NET6_0_OR_GREATER
            isAndroid = OperatingSystem.IsAndroid();
#endif

            IsAsyncCompatible = !isAndroid;
        }

        public static async Task Delay(int ms) => await Delay(ms, CancellationToken.None);

        public static async Task Delay(int ms, CancellationToken cancellationToken)
        {
            if (IsAsyncCompatible)
                await Task.Delay(ms, cancellationToken);
            else
            {
                Thread.Sleep(ms);
            }
        }

        public static async Task WriteAsync(Stream stream, byte[] data, int offset, int count) => await WriteAsync(stream, data, offset, count, CancellationToken.None);

        public static async Task WriteAsync(Stream stream, byte[] data, int offset, int count, CancellationToken cancellationToken)
        {
            if (IsAsyncCompatible)
                await stream.WriteAsync(data, offset, count, cancellationToken);
            else
            {
                stream.Write(data, offset, count);
            }
        }

        public static async Task ReadAsync(Stream stream, byte[] data, int offset, int count) => await ReadAsync(stream, data, offset, count, CancellationToken.None);

        public static async Task ReadAsync(Stream stream, byte[] data, int offset, int count, CancellationToken cancellationToken)
        {
            if (IsAsyncCompatible)
                await stream.ReadAsync(data, offset, count, cancellationToken);
            else
            {
                stream.Read(data, offset, count);
            }
        }

        public static async Task<string> ReadLineAsync(TextReader textReader)
        {
            if (IsAsyncCompatible)
            {
                return await textReader.ReadLineAsync();
            }
            else
            {
                return textReader.ReadLine();
            }
        }
    }
}