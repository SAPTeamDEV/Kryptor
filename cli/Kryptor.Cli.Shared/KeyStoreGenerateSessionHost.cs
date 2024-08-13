using System;
using System.IO;

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
            Size = size;
            Token = token;
            Margin = magin;
            Output = output;
        }

        public override void Start(ClientContext context)
        {
            base.Start(context);

            Log("This feature is not completed and may have bugs");

            KeyStoreLoadSession ksLoader;

            if (Generator != KeyStoreGenerator.None)
            {
                string kSize = Margin > 0 ? $"{Size}+{Margin}" : $"{Size}";
                Log($"Generating keystore with {kSize} keys using {Generator}");
                ksLoader = new KeyStoreRandomLoadSession(Generator, Size, Margin);
            }
            else
            {
                ITranformer tranformer = Transformers.GetTranformer(Token);
                string kSize = Margin > 0 ? $"{Token.KeySize}+{Margin}" : $"{Token.KeySize}";
                Log($"Generating keystore with {kSize} keys using {tranformer.GetType().Name}");
                ksLoader = new KeyStoreTokenLoadSession(Token, Margin);
            }

            NewSession(ksLoader);
            ShowProgressMonitored(true).Wait();

            if (ksLoader.EndReason == SessionEndReason.Completed)
            {
                KeyStore ks = ksLoader.KeyStore;

                Log($"Keystore fingerprint: {ks.Fingerprint.FormatFingerprint()}");
                if (string.IsNullOrEmpty(Output))
                {
                    Output = BitConverter.ToString(ks.Fingerprint).Replace("-", "").ToLower() + ".kks";
                }

                File.WriteAllBytes(Output, ks.Raw);
            }
        }
    }
}