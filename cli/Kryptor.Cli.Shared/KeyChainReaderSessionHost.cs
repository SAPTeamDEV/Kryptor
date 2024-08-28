using System;
using System.IO;

using SAPTeam.Kryptor.Client;

namespace SAPTeam.Kryptor.Cli
{
    public class KeyChainReaderSessionHost : CliSessionHost
    {
        public string KeyChainPath;
        public string FilePath;

        public KeyChainReaderSessionHost(GlobalOptions globalOptions, string keyChainPath, string filePath) : base(globalOptions)
        {
            KeyChainPath = keyChainPath;
            FilePath = filePath;
        }

        public override void Start(ClientContext context)
        {
            base.Start(context);

            if (!File.Exists(FilePath))
            {
                throw new FileNotFoundException(FilePath);
            }

            KeyChainCollection keyChainCollection = new KeyChainCollection(KeyChainPath);

            DebugLog("Reading header");

            CliHeader header = Header.ReadHeader<CliHeader>(File.OpenRead(FilePath));

            if (string.IsNullOrEmpty(header.Serial))
            {
                LogError("Cannot find file serial key");
                return;
            }

            Log($"File Serial Key: {header.Serial}");

            KeyChain keyChain;
            try
            {
                keyChain = keyChainCollection[header.Serial];
            }
            catch
            {
                LogError("Cannot find a key chain with this serial");
                return;
            }

            Log($"Keystore Fingerprint: {keyChain.Fingerprint.FormatFingerprint()}");
            string ksFileName = Path.Combine(Path.GetDirectoryName(keyChainCollection.FilePath), BitConverter.ToString(keyChain.Fingerprint).Replace("-", "").ToLower() + ".kks");

            if (!string.IsNullOrEmpty(keyChain.TransformerToken))
            {
                Log($"Transformer Token: {keyChain.TransformerToken}");
            }
            else if (File.Exists(ksFileName))
            {
                Log($"Keystore File Path: {ksFileName}");
            }
        }
    }
}