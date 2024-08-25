using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPTeam.Kryptor.Cli
{
    public partial class CliContext
    {
        public string Variant => "Legacy";

        public string FrameworkType => "Framework";

        public string FrameworkVersion =>
#if NET462
            "4.6.2"
#elif NET472
            "4.7.2"
#elif NET481
            "4.8.1"
#endif
            ;
    }
}
