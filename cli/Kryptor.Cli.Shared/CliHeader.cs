using System;
using System.Collections.Generic;
using System.Reflection;

using SAPTeam.Kryptor.Client;

namespace SAPTeam.Kryptor.Cli
{
    public class CliHeader : ClientHeader
    {
        public static ClientHeader Create()
        {
            Dictionary<string, string> extra = new Dictionary<string, string>
            {
                ["client"] = "kryptor-cli"
            };

            return new CliHeader()
            {
                ClientName = "Kryptor Cli",
                ClientVersion = new Version(Assembly.GetAssembly(typeof(Program)).GetCustomAttribute<AssemblyFileVersionAttribute>().Version),
                Extra = extra
            };
        }
    }
}