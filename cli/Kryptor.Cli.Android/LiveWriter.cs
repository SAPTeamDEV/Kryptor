using System.Text;

namespace SAPTeam.Kryptor.Cli
{
    internal class LiveWriter : TextWriter
    {
        private readonly TextView output;
        private readonly StringBuilder sb;

        public LiveWriter(TextView textView)
        {
            sb = new StringBuilder();
            output = textView;
        }

        private void ResetBuffer() => sb.Clear();

        public override Encoding Encoding => Encoding.UTF8;

        public override void Write(char value) => sb.Append(value);

        public override void Flush()
        {
            output.Text += sb.ToString();
            ResetBuffer();

            int scrollAmount = output.Layout.GetLineTop(output.LineCount) - output.Height;
            // if there is no need to scroll, scrollAmount will be <=0
            if (scrollAmount > 0)
                output.ScrollTo(0, scrollAmount);
            else
                output.ScrollTo(0, 0);
        }
    }
}
