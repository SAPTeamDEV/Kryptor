using System;
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
                var data = reader.ReadToEnd();
                if (string.IsNullOrEmpty(data))
                {
                    data = "{}";
                }

                Index = ClientTypesJsonWorker.ReadJson<WordlistIndex>(data);
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