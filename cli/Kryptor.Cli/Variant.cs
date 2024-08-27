using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPTeam.Kryptor.Cli
{
    internal static partial class BuildInformation
    {
        public static BuildVariant Variant => BuildVariant.Standard;

        public static bool IsAot => false;
    }
}
