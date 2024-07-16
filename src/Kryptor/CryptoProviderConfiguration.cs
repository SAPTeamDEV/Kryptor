using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPTeam.Kryptor
{
    public class CryptoProviderConfiguration : ICloneable
    {
        /// <summary>
        /// Gets or sets the crypto provider identifier.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets the configuration of continuous encryption method.
        /// </summary>
        public virtual bool Continuous { get; set; }

        /// <summary>
        /// Gets the configuration of remove hash feature.
        /// </summary>
        public virtual bool RemoveHash { get; set; }

        /// <summary>
        /// Gets the configuration of dynamic block processing feature.
        /// </summary>
        public virtual bool DynamicBlockProccessing { get; set; }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
