namespace SAPTeam.Kryptor
{
    /// <summary>
    /// Provides standard to hold the crypto providers configuration.
    /// </summary>
    public class CryptoProviderConfiguration : ICloneable, IEquatable<CryptoProviderConfiguration>
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
        public virtual bool DynamicBlockProcessing { get; set; }

        /// <inheritdoc/>
        public object Clone() => MemberwiseClone();

        /// <inheritdoc/>
        public bool Equals(CryptoProviderConfiguration other)
        {
            return Id == other.Id
                && Continuous == other.Continuous
                && RemoveHash == other.RemoveHash
                && DynamicBlockProcessing == other.DynamicBlockProcessing;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) => obj is CryptoProviderConfiguration other && Equals(other);

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return Id.GetHashCode()
                 & Continuous.GetHashCode()
                 & RemoveHash.GetHashCode()
                 & DynamicBlockProcessing.GetHashCode();
        }
    }
}
