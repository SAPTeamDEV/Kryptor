using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SAPTeam.Kryptor.Client
{
    /// <summary>
    /// Represents a collection to work with key chains.
    /// </summary>
    public class KeyChainCollection
    {
        private readonly object _saveLock = new object();

        private readonly List<KeyChain> keyChainList;

        /// <summary>
        /// Gets the path of the keychain json file.
        /// </summary>
        public string FilePath { get; }

        /// <summary>
        /// Gets the keychain with the specified <paramref name="serial"/>.
        /// </summary>
        /// <param name="serial">
        /// The unique identifier of the file.
        /// </param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        /// <exception cref="ApplicationException"></exception>
        public KeyChain this[string serial]
        {
            get
            {
                IEnumerable<KeyChain> entries = keyChainList.Where(x => x.Serial == serial);
                if (!entries.Any())
                {
                    throw new KeyNotFoundException(serial);
                }
                else if (entries.Count() > 1)
                {
                    throw new ApplicationException("Illegal douplication found in keychain");
                }

                return entries.First();
            }
        }

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
                FilePath = path;
            }
            else if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            if (Directory.Exists(path))
            {
                FilePath = Path.Combine(path, "keychain.json");
            }

            string data = null;
            using (StreamReader reader = new StreamReader(File.Open(FilePath, FileMode.OpenOrCreate)))
            {
                data = reader.ReadToEnd();
                if (string.IsNullOrEmpty(data))
                {
                    data = "[]";
                }
            }

            keyChainList = ClientTypesJsonWorker.ReadJson<List<KeyChain>>(data);
        }

        /// <summary>
        /// Adds a new keychain to the collection.
        /// </summary>
        /// <param name="header">
        /// The header of the encrypted file.
        /// </param>
        /// <param name="keyStore">
        /// The keystore used to encrypt the file.
        /// </param>
        /// <param name="transformerToken">
        /// The transformer token used to generate the keystore (if presents).
        /// </param>
        /// <exception cref="ArgumentException"></exception>
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

            string ksFileName = Path.Combine(Path.GetDirectoryName(FilePath), BitConverter.ToString(keyStore.Fingerprint).Replace("-", "").ToLower() + ".kks");
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
                string kJson = ClientTypesJsonWorker.ToJson(keyChainList);

                File.WriteAllText(FilePath, kJson);
            }
        }
    }
}
