using System;
using System.CommandLine;

namespace SAPTeam.Kryptor.Cli
{
    public class CommandLineParser
    {
        public CommandLineParser(string[] args)
        {
            var root = new RootCommand("Kryptor Command-Line Interface");

            #region Common Data Processing Options
            var blockSize = new Option<int>("--block-size", () => Kes.DefaultBlockSize, "Determines the block size for data processing");
            blockSize.AddAlias("-b");

            var provider = new Option<string>("--provider", () => "3", "Determines the crypto provider to process data");
            provider.AddAlias("-p");

            var continuous = new Option<bool>("--continuous", "Enables using the Continuous method");
            continuous.AddAlias("-c");

            var removeHash = new Option<bool>("--remove-hash", "Removes the block hash to increase the security");
            removeHash.AddAlias("-r");

            var dbp = new Option<bool>("--dbp", "Enables the Dynamic Block Processing");
            dbp.AddAlias("-d");

            var keystore = new Option<string>("--keystore", "Keystore file path or transformer token to encrypt/decrypt data");
            keystore.AddAlias("-k");
            keystore.IsRequired = true;

            var files = new Argument<string[]>("files", "Files to be processed");
            #endregion

            #region Encryption Options
            var hVerbose = new Option<int>("--header", () => 2, "Determines the amount of data stored in the header. 0 means no data and 3 means all data needed to decrypt the file (except the keystore)");

            var encCmd = new Command("encrypt", "Encrypts files with keystore")
            {
                blockSize,
                provider,
                continuous,
                removeHash,
                dbp,
                hVerbose,
                keystore,
                files
            };

            encCmd.AddAlias("e");
            encCmd.AddAlias("enc");

            encCmd.SetHandler((blockSizeT, providerT, continuousT, removeHashT, dbpT, hVerboseT, keystoreT, filesT) =>
            {
                var sessionHost = new EncryptionSessionHost(blockSizeT, providerT, continuousT, removeHashT, dbpT, keystoreT, filesT, hVerboseT);
                Program.Context.NewSessionHost(sessionHost);
            }, blockSize, provider, continuous, removeHash, dbp, hVerbose, keystore, files);

            root.AddCommand(encCmd);
            #endregion

            #region Decryption Options
            var decCmd = new Command("decrypt", "Decrypts files with keystore")
            {
                blockSize,
                provider,
                continuous,
                removeHash,
                dbp,
                keystore,
                files
            };

            decCmd.AddAlias("d");
            decCmd.AddAlias("dec");

            decCmd.SetHandler((p, c, r, d, k, f) =>
            {
                Console.WriteLine("Provider: " + CryptoProviderFactory.GetDisplayName(p));
                Console.WriteLine($"Continuous: {c}");
                Console.WriteLine($"Remove Hash: {r}");
                Console.WriteLine($"Dynamic Block Processing: {d}");
                Console.WriteLine($"Keystore: {k}");
                Console.WriteLine($"Files:\n{string.Join(Environment.NewLine, f)}");
            }, provider, continuous, removeHash, dbp, keystore, files);

            root.AddCommand(decCmd);
            #endregion

            //ot.SetHandler(() => Program.Context.NewSessionHost(new CliSessionHost()));

            root.Invoke(args);
        }
    }
}