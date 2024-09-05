using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPTeam.Kryptor.Cli
{
    internal class LiveWriter : TextWriter
    {
        TextView output;
        char[] buffer;
        int pos;

        public LiveWriter(TextView textView)
        {
            output = textView;
            ResetBuffer();
        }

        void ResetBuffer()
        {
            buffer = new char[1024];
            pos = 0;
        }

        public override Encoding Encoding => Encoding.UTF8;

        public override void Write(char value)
        {
            buffer[pos++] = value;

            if (pos == buffer.Length)
            {
                Flush();
            }
        }

        public override void Flush()
        {
            output.Text += new string(buffer);
            ResetBuffer();
        }
    }
}
