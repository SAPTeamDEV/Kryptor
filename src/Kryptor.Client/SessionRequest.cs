namespace SAPTeam.Kryptor.Client
{
    /// <summary>
    /// Represents a class for communicattion between the session and the user.
    /// </summary>
    /// <typeparam name="TResponse">
    /// The type of requested response from user.
    /// </typeparam>
    public class SessionRequest<TResponse>
    {
        /// <summary>
        /// Gets the message of the request.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Gets the default value of the request.
        /// </summary>
        public TResponse DefaultValue { get; }

        /// <summary>
        /// Gets or sets the user response.
        /// </summary>
        public TResponse Response { get; set; }

        /// <summary>
        /// Initializes a new request.
        /// </summary>
        /// <remarks>
        /// This class just holds the data. To send the request to the user, an instance of this class must be sent to the <see cref="ISessionHost"/>.
        /// </remarks>
        /// <param name="message">
        /// The message of the request
        /// </param>
        /// <param name="defaultValue">
        /// The default value for this request. This value used in some cases to set the response.
        /// </param>
        public SessionRequest(string message, TResponse defaultValue)
        {
            Message = message;
            DefaultValue = defaultValue;
        }
    }
}
