using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPTeam.Kryptor.Tool
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
