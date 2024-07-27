using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPTeam.Kryptor.Client
{
    /// <summary>
    /// Provides common utilities used by kryptor front ends.
    /// </summary>
    public static class Utilities
    {
        public static string GetShortVersionString(string verStr)
        {
            var ver = new Version(verStr);
            return string.Join(".", ver.Major, ver.Minor, ver.Build);
        }
    }
}
