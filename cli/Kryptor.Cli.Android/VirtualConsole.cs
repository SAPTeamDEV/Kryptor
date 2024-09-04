using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPTeam.Kryptor.Cli
{
    internal class VirtualConsole : IConsole
    {
        public IStandardStreamWriter Out { get; }
        public bool IsOutputRedirected => true;
        public IStandardStreamWriter Error { get; }
        public bool IsErrorRedirected => true;
        public bool IsInputRedirected => true;

        public VirtualConsole(StreamWriter sw)
        {
            IStandardStreamWriter issw = new SSW(sw);
            Out = issw;
            Error = issw;
        }
    }

    internal class SSW : IStandardStreamWriter
    {
        StreamWriter inner;

        public void Write(string value) => inner.Write(value);

        public SSW(StreamWriter streamWriter)
        {
            inner = streamWriter;
        }
    }
}
