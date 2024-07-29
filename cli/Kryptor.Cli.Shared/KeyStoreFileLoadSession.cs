using System.Threading;
using System.Threading.Tasks;

using SAPTeam.Kryptor.Client;

namespace SAPTeam.Kryptor.Cli
{
    public class KeyStoreFileLoadSession : Session
    {
        public override double Progress { get; protected set; }
        public override string Description { get; protected set; }
        protected override Task<bool> RunAsync(CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}