using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace SAPTeam.Kryptor.Cli
{
    public static class Holder
    {
        internal static Stopwatch ProcessTime;
        internal static CancellationTokenSource Token;
        internal static Task Task;
    }
}
