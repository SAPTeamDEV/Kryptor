namespace SAPTeam.Kryptor.Cli
{
    public class PauseRequest
    {
        public string Message { get; set; }

        public bool Response { get; private set; }

        public bool IsResponsed { get; private set; }

        public PauseRequest(string message)
        {
            Message = message;
            Response = false;
            IsResponsed = false;
        }

        public void SetResponse(bool response)
        {
            IsResponsed = true;
            Response = response;
        }

        public bool IsEmpty() => string.IsNullOrEmpty(Message);
    }
}

