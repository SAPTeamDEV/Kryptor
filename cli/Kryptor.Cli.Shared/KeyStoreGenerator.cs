namespace SAPTeam.Kryptor.Cli
{
    public enum KeyStoreGenerator
    {
        None = 0,
        CryptoRng,
        Unix,
        SafeRng,
        EntroX,
    }
}