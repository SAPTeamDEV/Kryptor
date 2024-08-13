using SAPTeam.Kryptor.Client;

namespace SAPTeam.Kryptor.Cli
{
    public class KeyStoreGenerateSessionHost : CliSessionHost
    {
        private KeyStoreGenerator Generator;
        private int Size;
        private TransformerToken Token;
        private int Margin;
        private string Output;

        public KeyStoreGenerateSessionHost(GlobalOptions globalOptions, KeyStoreGenerator generator, int size, TransformerToken token, int magin, string output) : base(globalOptions)
        {
            Generator = generator;
            Size = size > 0 ? size : KeyStore.GetRandomOddNumber();
            Token = token;
            Margin = magin;
            Output = output;
        }

        public override void Start(ClientContext context)
        {
            base.Start(context);

            KeyStoreLoadSession ksLoader;

            if (Token.IsValid())
            {
                ITranformer tranformer = Transformers.GetTranformer(Token);
                string kSize = Margin > 0 ? $"{Token.KeySize}+{Margin}" : $"{Token.KeySize}";
                Log($"Generating keystore with {kSize} keys using {tranformer.GetType().Name}");
                ksLoader = new KeyStoreTokenLoadSession(true, Token, Margin);
            }
            else
            {
                string kSize = Margin > 0 ? $"{Size}+{Margin}" : $"{Size}";
                Log($"Generating keystore with {kSize} keys using {Generator}");
                ksLoader = new KeyStoreRandomLoadSession(true, Generator, Size, Margin);
            }

            var fileWriter = new FileSaveSession(Output, null);
            ksLoader.ContinueWith(fileWriter);

            NewSession(ksLoader);
            NewSession(fileWriter);

            ShowProgressMonitored(true).Wait();
        }
    }
}