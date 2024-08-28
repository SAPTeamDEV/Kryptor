using System;
using System.IO;

using SAPTeam.Kryptor.Client;

namespace SAPTeam.Kryptor.Cli
{
    public class HeaderReaderSessionHost : CliSessionHost
    {
        public string FilePath;

        public HeaderReaderSessionHost(GlobalOptions globalOptions, string filePath) : base(globalOptions) => FilePath = filePath;

        public override void Start(ClientContext context)
        {
            base.Start(context);

            if (!File.Exists(FilePath))
            {
                throw new FileNotFoundException(FilePath);
            }

            DebugLog("Reading header");

            CliHeader header = Header.ReadHeader<CliHeader>(File.OpenRead(FilePath));

            if (header.Verbosity == 0)
            {
                LogError("Cannot find any header data");
                return;
            }

            Log();
            Log($"Header informations of {Path.GetFileName(FilePath)} with {header.Verbosity} verbosity");
            Log($"API Version: {header.Version.ToString(2)}");
            Log($"Engine Version: {header.EngineVersion}");
            Log();
            Log($"Client Name: {header.ClientName}");
            Log($"Client Version: {header.ClientVersion}");

            if ((int)header.Verbosity > 1)
            {
                Log();
                Log($"Original File Name: {header.FileName}");
                Log($"Serial Key: {header.Serial}");
                Log();
                Log($"Block Size: {header.BlockSize}");

                if (header.Configuration != null)
                {
                    Log($"Crypto Provider: {(Verbose ? CryptoProviderFactory.GetRegisteredCryptoProviderId(header.Configuration.Id) : CryptoProviderFactory.GetDisplayName(header.Configuration.Id))}");
                    Log($"Continuous: {header.Configuration.Continuous}");
                    Log($"Remove Hash: {header.Configuration.RemoveHash}");
                    Log($"Dynamic Block Processing: {header.Configuration.DynamicBlockProccessing}");
                }
            }

            if (Verbose && header.Extra != null)
            {
                Log();
                Log($"Extra data:\n{string.Join(Environment.NewLine, header.Extra)}");
            }
        }
    }
}