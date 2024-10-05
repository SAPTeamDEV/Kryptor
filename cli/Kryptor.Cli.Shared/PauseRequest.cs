namespace SAPTeam.Kryptor.Cli
{
    public class PauseRequest
    {
        public string Message { get; set; }

        public bool Default { get; }

        public bool Response { get; private set; }

        public bool IsResponded { get; private set; }

        public PauseRequest(string message, bool defaultValue)
        {
            Message = message;
            Default = defaultValue;
            IsResponded = false;
        }

        public void SetResponse(bool response)
        {
            IsResponded = true;
            Response = response;
        }

        public bool IsEmpty() => string.IsNullOrEmpty(Message);
    }
}

