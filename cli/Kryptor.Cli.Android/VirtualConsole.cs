using System.CommandLine;
using System.CommandLine.IO;

namespace SAPTeam.Kryptor.Cli
{
    internal class VirtualConsole : IConsole
    {
        public IStandardStreamWriter Out { get; }
        public bool IsOutputRedirected => true;
        public IStandardStreamWriter Error { get; }
        public bool IsErrorRedirected => true;
        public bool IsInputRedirected => true;

        public VirtualConsole(TextWriter sw)
        {
            IStandardStreamWriter issw = new SSW(sw);
            Out = issw;
            Error = issw;
        }
    }

    internal class SSW : IStandardStreamWriter
    {
        private readonly TextWriter inner;

        public void Write(string value) => inner.Write(value);

        public SSW(TextWriter streamWriter) => inner = streamWriter;
    }
}
