namespace SAPTeam.Kryptor.Cli
{
    public class ConsoleFrameBuffer
    {
        private readonly string[] LoadingFrameBuffer = new string[]
        {
            "||--",
            "|||-",
            "-|||",
            "--||",
            "---|",
            "--||",
            "-|||",
            "|||-",
            "||--",
            "|---",
        };
        private readonly string[] WaitingFrameBuffer = new string[]
        {
            "|---",
            "||--",
            "|||-",
            "||||",
            "-|||",
            "--||",
            "---|",
            "----",
            "----",
            "----",
            "----",
        };
        private readonly string[] PausedFrameBuffer = new string[]
        {
            "||||",
            "||||",
            "----",
            "----",
            "----",
            "----",
        };
        private int loadingPos = 0;
        private int waitingPos = 0;
        private int pausedPos = 0;

        public string Loading => LoadingFrameBuffer[loadingPos];
        public string Waiting => WaitingFrameBuffer[waitingPos];
        public string Paused => PausedFrameBuffer[pausedPos];

        public void Next()
        {
            loadingPos = (loadingPos + 1) % LoadingFrameBuffer.Length;
            waitingPos = (waitingPos + 1) % WaitingFrameBuffer.Length;
            pausedPos = (pausedPos + 1) % PausedFrameBuffer.Length;
        }
    }
}