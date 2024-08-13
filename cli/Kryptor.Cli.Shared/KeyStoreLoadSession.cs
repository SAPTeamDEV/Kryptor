using SAPTeam.Kryptor.Client;

namespace SAPTeam.Kryptor.Cli
{
    public abstract class KeyStoreLoadSession : Session
    {
        public KeyStore KeyStore { get; protected set; }

        protected bool ShowFingerprint { get; }

        protected KeyStoreLoadSession(bool showFingerprint)
        {
            ShowFingerprint = showFingerprint;
        }

        protected void SetEndDescription()
        {
            if (ShowFingerprint)
            {
                Description = $"Keystore fingerprint: {KeyStore.Fingerprint.FormatFingerprint()}";
            }
            else
            {
                Description = "Keystore loaded";
            }
        }
    }
}