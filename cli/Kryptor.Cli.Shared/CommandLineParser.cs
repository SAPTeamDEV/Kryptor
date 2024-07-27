using System;
using System.CommandLine;
using System.Drawing;

namespace SAPTeam.Kryptor.Cli
{
    public class CommandLineParser
    {
        public CommandLineParser(string[] args)
        {
            var providerOpt = new Option<string>("--provider", () => "3", "Determines the crypto provider to process data");
            providerOpt.AddAlias("-p");

            var continuous = new Option<bool>("--continuous", "Enables using the Continuous method");
            continuous.AddAlias("-c");

            var removeHash = new Option<bool>("--remove-hash", "Removes the block hash to increase the security");
            removeHash.AddAlias("-r");

            var dbp = new Option<bool>("--dbp", "Enables the Dynamic Block Processing");
            dbp.AddAlias("-d");

            var hVerbose = new Option<int>("--header-verbosity", () => 2, "Determines the amount of data stored in the header. 0 means no data and 3 means all data needed to decrypt the file (except the keystore)");
            hVerbose.AddAlias("-h");

            var root = new RootCommand("Kryptor Command-Line Interface");

            var encCmd = new Command("encrypt", "Encrypts files with keystore")
            {
                providerOpt,
                continuous,
                removeHash,
                dbp,
                hVerbose
            };
            encCmd.AddAlias("e");
            encCmd.AddAlias("enc");
            encCmd.SetHandler((p, c, r, d, h) =>
            {
                Console.WriteLine("Provider: " + CryptoProviderFactory.GetDisplayName(p));
                Console.WriteLine($"Continuous: {c}");
                Console.WriteLine($"Remove Hash: {r}");
                Console.WriteLine($"Dynamic Block Processing: {d}");
                Console.WriteLine($"Header Verbose: {h}");
            }, providerOpt, continuous, removeHash, dbp, hVerbose);

            root.AddCommand(encCmd);
            root.SetHandler(() => Program.Context.NewSessionHost(new CliSessionHost()));

            root.Invoke(args);
        }
    }
}