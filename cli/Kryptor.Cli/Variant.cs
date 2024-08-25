using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPTeam.Kryptor.Cli
{
    public partial class CliContext
    {
        public string Variant => "Standard";

        public string Framework =>
#if NET6_0
            "6.0"
#elif NET8_0
            "8.0"
#endif
            ;
    }
}
