namespace SAPTeam.Kryptor.Helpers
{
    /// <summary>
    /// Represents interface to report progress of works.
    /// </summary>
    public interface IProgressReport
    {
        /// <summary>
        /// Triggered when the work progress is changed.
        /// </summary>
        /// <remarks>
        /// Expected values are 0 - 100 and -1. -1 means unspecifed exact progress value, but the work is being done.
        /// </remarks>
        event EventHandler<double> ProgressChanged;
    }
}
