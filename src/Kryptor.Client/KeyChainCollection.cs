using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Newtonsoft.Json;

namespace SAPTeam.Kryptor.Client
{
    public class KeyChainCollection
    {
        private object _saveLock = new object();

        private List<KeyChain> keyChainList;
        private string filePath;
        private JsonSerializerSettings settings = new JsonSerializerSettings()
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyChainCollection"/> class.
        /// </summary>
        /// <param name="path">
        /// The keychain file name or directory name to create of search for existing keychain file.
        /// </param>
        public KeyChainCollection(string path)
        {
            if (File.Exists(path))
            {
                filePath = path;
            }
            else if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            if (Directory.Exists(path))
            {
                filePath = Path.Combine(path, "keychain.json");
            }

            string data = null;
            using (var reader = new StreamReader(File.Open(filePath, FileMode.OpenOrCreate)))
            {
                data = reader.ReadToEnd();
                if (string.IsNullOrEmpty(data))
                {
                    data = "[]";
                }
            }

            keyChainList = JsonConvert.DeserializeObject<List<KeyChain>>(data);
        }

        public void Add(ClientHeader header, KeyStore keyStore, string transformerToken = null)
        {
            if (string.IsNullOrEmpty(header.Serial))
            {
                throw new ArgumentException("Invalid header serial");
            }

            if (keyChainList.Any(x => x.Serial == header.Serial))
            {
                throw new ArgumentException("A keychain with this serial is already exists");
            }

            string ksFileName = Path.Combine(Path.GetDirectoryName(filePath), BitConverter.ToString(keyStore.Fingerprint).Replace("-", "").ToLower() + ".kks");
            if (string.IsNullOrEmpty(transformerToken) && !File.Exists(ksFileName))
            {
                File.WriteAllBytes(ksFileName, keyStore.Raw);
            }

            keyChainList.Add(new KeyChain()
            {
                Serial = header.Serial,
                Fingerprint = keyStore.Fingerprint,
                TransformerToken = transformerToken
            });
        }

        /// <summary>
        /// Saves the keychain file.
        /// </summary>
        public void Save()
        {
            lock (_saveLock)
            {
                string kJson = JsonConvert.SerializeObject(keyChainList, settings);

                File.WriteAllText(filePath, kJson);
            }
        }
    }
}
