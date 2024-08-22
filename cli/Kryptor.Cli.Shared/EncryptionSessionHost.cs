using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Newtonsoft.Json;

using SAPTeam.Kryptor.Client;

namespace SAPTeam.Kryptor.Cli
{
    public class EncryptionSessionHost : DataProcessingSessionHost
    {
        public int HeaderVerbosity { get; }
        public string KeyChain { get; }
        public bool UseKeyChain { get; }

        public EncryptionSessionHost(GlobalOptions globalOptions, DataProcessingOptions options, int hVerbose, string keyChain) : base(globalOptions, options)
        {
            HeaderVerbosity = hVerbose;
            KeyChain = keyChain;
            UseKeyChain = !string.IsNullOrEmpty(keyChain);

        }

        public override void Start(ClientContext context)
        {
            base.Start(context);

            List<KeyChain> KeyChainList = null;
            if (UseKeyChain)
            {
                KeyChainList = JsonConvert.DeserializeObject<List<KeyChain>>(!File.Exists(KeyChain) ? "[]" : File.ReadAllText(KeyChain));
            }

            foreach (string file in Files)
            {
                EncryptionSession session = new EncryptionSession(KeyStore, Configuration, BlockSize, HeaderVerbosity, file);
                NewSession(session);
            }

            ShowProgressMonitored(true).Wait();

            if (UseKeyChain)
            {
                DebugLog("Collecting KeyChain informations");

                foreach (EncryptionSession s in Container.Sessions.OfType<EncryptionSession>().Where(x => x.EndReason == SessionEndReason.Completed))
                {
                    KeyChainList.Add(new KeyChain()
                    {
                        Serial = s.Header.Serial,
                        Fingerprint = KeyStore.Fingerprint,
                        KeyStoreHint = KeystoreString
                    });
                }

                DebugLog("Updating KeyChain data");

                var settings = new JsonSerializerSettings()
                {
                    Formatting = Formatting.Indented,
                    NullValueHandling = NullValueHandling.Ignore,
                };

                string kJson = JsonConvert.SerializeObject(KeyChainList, settings);
                byte[] kEncode = Encoding.UTF8.GetBytes(kJson);

                File.WriteAllBytes(KeyChain, kEncode);
            }
        }
    }
}