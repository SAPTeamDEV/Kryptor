using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPTeam.Kryptor.Cli
{
    public partial class CliContext
    {
        public string Variant => "Native AOT";

        public string Framework =>
#if NET8_0
            "8.0"
#endif
            ;
    }
}
