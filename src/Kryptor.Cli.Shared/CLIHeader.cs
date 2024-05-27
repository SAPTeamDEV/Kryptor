using System;

namespace SAPTeam.Kryptor.Cli
{
    internal class CLIHeader : Header
    {
        /// <summary>
        /// Gets or sets the original name of file.
        /// </summary>
        public string OriginalName { get; set; }

        /// <summary>
        /// Gets or sets the version of Kryptor CLI.
        /// </summary>
        public Version CliVersion { get; set; }
    }
}
