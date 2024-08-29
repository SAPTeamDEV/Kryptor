using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using SAPTeam.Kryptor.Extensions;

namespace SAPTeam.Kryptor.Client.Security
{
    /// <summary>
    /// Represents schema to store and retrive wordlist informations.
    /// </summary>
    public class WordlistIndexEntry
    {
        private WordlistVerificationMetadata[] metadata;

        /// <summary>
        /// Gets or sets the identifier of the wordlist.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the user-friendly name of the wordlist.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the URI used to download the wordlist.
        /// </summary>
        public Uri Uri { get; set; }

        /// <summary>
        /// Gets or sets the SHA256 hash of the wordlist.
        /// </summary>
        public byte[] Hash { get; set; }

        /// <summary>
        /// Gets or sets the enforcement status of the wordlist. if it's true, it will block any operations if the word is found in the wordlist, but if set to false, it just shows a warning.
        /// </summary>
        public bool Enforced { get; set; }

        /// <summary>
        /// Gets or sets the compressed status of the wordlist file. if it's true, the downloader will try to decompress it, otherwise it will be processed as is.
        /// </summary>
        public bool Compressed { get; set; }

        /// <summary>
        /// Gets or sets the optimization status of the wordlist file. if it's true, it means that the wordlist is optimized by fragment compiler and all duplicated entries are removed, otherwise it means that the wordlist is not optimized and may be have some duplicated entries.
        /// </summary>
        public bool Optimized { get; set; }

        /// <summary>
        /// Gets or sets the install directory of thw wordlist.
        /// </summary>
        public string InstallDirectory { get; set; }

        /// <summary>
        /// Gets or sets the number of lines in this wordlist.
        /// </summary>
        public long Lines { get; set; }

        /// <summary>
        /// Gets or sets the number of actual words in this wordlist.
        /// </summary>
        public long Words { get; set; }

        /// <summary>
        /// Gets or sets the wordlist file size.
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// Opens the wordlist with <see cref="WordlistFragmentCollection"/>.
        /// </summary>
        /// <returns>
        /// A new instance of the <see cref="WordlistFragmentCollection"/> class initialized with this wordlist.
        /// </returns>
        public WordlistFragmentCollection Open()
        {
            CheckBasicValidity(true);
            CheckInstallation(true);

            return new WordlistFragmentCollection(this);
        }

        /// <summary>
        /// Gets the metadata array of this wordlist.
        /// </summary>
        /// <returns>
        /// An array contains the metadata for all fragments.
        /// </returns>
        public WordlistVerificationMetadata[] GetMetadata()
        {
            if (metadata == null || metadata.Length == 0)
            {
                string json = File.ReadAllText(GetMetadataPath());
                List<WordlistVerificationMetadata> data = ClientTypesJsonWorker.ReadJson<List<WordlistVerificationMetadata>>(json);
                metadata = data.ToArray();
            }

            return metadata;
        }

        private string GetMetadataPath() => Path.Combine(InstallDirectory, "metadata.json");

        /// <summary>
        /// Checks basic validity of this instance, includes valid id and hash.
        /// </summary>
        /// <param name="throwException">
        /// if <see langword="true"/>, the method will throw exception when any of the checking items were failed. otherwise it just returns the validation result as <see langword="bool"/>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if validation were passed. otherwise it will return <see langword="false"/>.
        /// </returns>
        /// <exception cref="InvalidDataException"></exception>
        public bool CheckBasicValidity(bool throwException = false)
        {
            bool isValid = true;

            if (string.IsNullOrEmpty(Id))
            {
                isValid = false;
                if (throwException) throw new InvalidDataException("The wordlist id could not be empty");
            }
            else if (Hash == null || Hash.Length == 0)
            {
                isValid = false;
                if (throwException) throw new InvalidDataException("The wordlist hash could not be empty");
            }

            return isValid;
        }

        /// <summary>
        /// Checks validity of the wordlist installation.
        /// </summary>
        /// <param name="throwException">
        /// if <see langword="true"/>, the method will throw exception when any of the checking items were failed. otherwise it just returns the validation result as <see langword="bool"/>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if validation were passed. otherwise it will return <see langword="false"/>.
        /// </returns>
        /// <exception cref="ApplicationException"></exception>
        /// <exception cref="DirectoryNotFoundException"></exception>
        /// <exception cref="InvalidDataException"></exception>
        /// <exception cref="FileNotFoundException"></exception>
        public bool CheckInstallation(bool throwException = false)
        {
            bool isInstalled = true;

            if (string.IsNullOrEmpty(InstallDirectory))
            {
                isInstalled = false;
                if (throwException) throw new ApplicationException("The wordlist is not installed");
            }
            else if (!Directory.Exists(InstallDirectory))
            {
                isInstalled = false;
                if (throwException) throw new DirectoryNotFoundException(InstallDirectory);
            }
            else if (Directory.GetFiles(InstallDirectory).Length == 0)
            {
                isInstalled = false;
                if (throwException) throw new InvalidDataException("The wordlist installation directory is empty");
            }
            else if (!File.Exists(GetMetadataPath()))
            {
                isInstalled = false;
                if (throwException) throw new FileNotFoundException("Cannont find the metadata file");
            }

            return isInstalled;
        }

        /// <summary>
        /// Verifies integrity and compatibility of the wordlist and it's fragments.
        /// </summary>
        /// <remarks>
        /// This method also calls the both <see cref="CheckBasicValidity(bool)"/> and <see cref="CheckInstallation(bool)"/> methods.
        /// </remarks>
        /// <param name="throwException">
        /// if <see langword="true"/>, the method will throw exception when any of the checking items were failed. otherwise it just returns the validation result as <see langword="bool"/>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if validation were passed. otherwise it will return <see langword="false"/>.
        /// </returns>
        /// <exception cref="InvalidDataException"></exception>
        public bool Verify(bool throwException = false)
        {
            bool basicChecksPassed = CheckBasicValidity(throwException) && CheckInstallation(throwException);

            if (!basicChecksPassed) return false;

            bool isVerified = true;

            WordlistVerificationMetadata[] metadata = GetMetadata();

            foreach (WordlistVerificationMetadata fragmentData in metadata)
            {
                bool validFragment = true;
                string fragmentPath = Path.Combine(InstallDirectory, fragmentData.FragmentId.ToString());

                if (!File.Exists(fragmentPath))
                {
                    validFragment = false;
                }
                else if (fragmentData.FragmentId != WordlistFragmentCollection.GetWordIdentifier(fragmentData.LookupString))
                {
                    validFragment = false;
                }
                else
                {
                    FileStream stream = File.OpenRead(fragmentPath);
                    byte[] hash = stream.Sha256();
                    byte[] checksum = Utilities.XOR(Hash, hash);

                    if (!fragmentData.Checksum.SequenceEqual(checksum))
                    {
                        validFragment = false;
                    }
                }

                if (!validFragment)
                {
                    isVerified = false;
                    break;
                }
            }

            return throwException && !isVerified ? throw new InvalidDataException("The wordlist is not verified") : isVerified;
        }
    }
}
