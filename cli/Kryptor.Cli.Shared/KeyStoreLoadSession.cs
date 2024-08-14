using System;
using System.Linq;

using MoreLinq;

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
            SessionEnded += TransferData;
        }

        private void TransferData(object sender, SessionEventArgs e)
        {
            if (e.EndReason == SessionEndReason.Completed)
            {
                Dependents.OfType<FileSaveSession>().ForEach(x =>
                {
                    if (string.IsNullOrEmpty(x.FilePath))
                    {
                        x.FilePath = BitConverter.ToString(KeyStore.Fingerprint).Replace("-", "").ToLower() + ".kks";
                    }

                    if (x.Data == null || x.Data.Length == 0)
                    {
                        x.Data = KeyStore.Raw;
                    }
                });
            }
        }

        protected void SetEndDescription() => Description = ShowFingerprint ? $"Keystore fingerprint: {KeyStore.Fingerprint.FormatFingerprint()}" : "Keystore loaded";
    }
}