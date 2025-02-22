using SAPTeam.Kryptor.Client;
using SAPTeam.Kryptor.Extensions;

namespace SAPTeam.Kryptor.Cli.KeyStoreAnalyze
{
    public class CrackSubSession : Session
    {
        public int Index;
        private readonly byte[] Test;

        public event Action OnVerify;

        public override bool IsHidden => true;

        public CrackSubSession(int index, byte[] test) : base()
        {
            Index = index;
            Test = test;
        }

        protected override async Task<bool> RunAsync(ISessionHost sessionHost, CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                byte[] buffer = new byte[3];
                buffer[0] = (byte)Index;

                for (int b1 = 0; b1 <= 255; b1++)
                {
                    for (int b2 = 0; b2 <= 255; b2++)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        buffer[1] = (byte)b1;
                        buffer[2] = (byte)b2;

                        if (buffer.Sha256().SequenceEqual(Test))
                        {
                            OnVerify?.Invoke();
                        }
                    }
                }
            });

            return true;
        }
    }
}