using SAPTeam.Kryptor.Client;

namespace SAPTeam.Kryptor.Cli
{
    public abstract class KeyStoreLoadSession : Session
    {
        public KeyStore KeyStore { get; protected set; }
    }
}