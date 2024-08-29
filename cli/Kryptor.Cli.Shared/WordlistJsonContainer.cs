using System.IO;

using SAPTeam.Kryptor.Client;
using SAPTeam.Kryptor.Client.Security;

namespace SAPTeam.Kryptor.Cli
{
    public class WordlistJsonContainer
    {
        object _lockObj = new object();
        string filePath;

        public WordlistIndex Index { get; }

        public WordlistJsonContainer(string filePath)
        {
            this.filePath = filePath;

            using (var reader = new StreamReader(File.Open(filePath, FileMode.OpenOrCreate, FileAccess.Read)))
            {
                Index = ClientTypesJsonWorker.ReadJson<WordlistIndex>(reader.ReadToEnd());
            }
        }

        public void Write()
        {
            lock (_lockObj)
            {
                var json = ClientTypesJsonWorker.ToJson(Index);

                File.WriteAllText(filePath, json);
            }
        }
    }
}