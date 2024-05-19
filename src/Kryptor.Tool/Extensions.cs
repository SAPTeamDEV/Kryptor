using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPTeam.Kryptor.Tool
{
    internal static class Extensions
    {
        internal static string FormatFingerprint(this byte[] src)
        {
            return BitConverter.ToString(src).Replace("-", ":");
        }
    }
}
