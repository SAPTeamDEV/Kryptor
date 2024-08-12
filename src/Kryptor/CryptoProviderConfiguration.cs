using System;

namespace SAPTeam.Kryptor
{
    /// <summary>
    /// Provides standard to hold the crypto providers configuration.
    /// </summary>
    public class CryptoProviderConfiguration : ICloneable
    {
        /// <summary>
        /// Gets or sets the crypto provider identifier/hint.
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

        /// <inheritdoc/>
        public object Clone() => MemberwiseClone();
    }
}
