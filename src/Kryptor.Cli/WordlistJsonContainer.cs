using SAPTeam.Kryptor.Client;
using SAPTeam.Kryptor.Client.Security;

namespace SAPTeam.Kryptor.Cli
{
    public class WordlistJsonContainer
    {
        private readonly object _lockObj = new object();
        private readonly string filePath;

        public WordlistIndex Index { get; }

        public WordlistJsonContainer(string filePath)
        {
            this.filePath = filePath;

            using (StreamReader reader = new StreamReader(File.Open(filePath, FileMode.OpenOrCreate, FileAccess.Read)))
            {
                string data = reader.ReadToEnd();
                if (string.IsNullOrEmpty(data))
                {
                    data = "{}";
                }

                Index = Utilities.ClientTypesJsonWorker.ReadJson<WordlistIndex>(data);
            }
        }

        public void Write()
        {
            lock (_lockObj)
            {
                string json = Utilities.ClientTypesJsonWorker.ToJson(Index);

                File.WriteAllText(filePath, json);
            }
        }
    }
}