using CommandLine.Text;

namespace SAPTeam.Kryptor.Tool
{
    internal class Arguments
    {
        [Usage(ApplicationAlias = "Kryptor")]
        public static IEnumerable<Example> Examples => new List<Example>()
        {
            new("Encrypt a file", new Arguments() { Encrypt = true, KeyStore = "keystore.kks", File = new string[] { "file", "file2", "..." } }),
            new("Decrypt a file", new Arguments() { Decrypt = true, KeyStore = "keystore.kks", File = new string[] { "file", "file2", "..." } }),
            new("Generate new keystore", new Arguments() { Generate = true, KeyStoreSize = 256, KeyStore = "keystore.kks" })
        };

        [Option('e', "encrypt", SetName = "encrypt", Required = true, HelpText = "Encrypts the given file with given keystore.")]
        public bool Encrypt { get; set; }

        [Option('d', "decrypt", SetName = "decrypt", Required = true, HelpText = "Decrypts the given file with given keystore.")]
        public bool Decrypt { get; set; }

        [Option('g', "generate", SetName = "generate", Required = true, HelpText = "Generates a new keystore file.")]
        public bool Generate { get; set; }

        [Option('k', "keystore-size", SetName = "generate", Default = 256, HelpText = "Specifies the number of keys to be generated for new keystore (Usable with -g). Default value is 256.")]
        public int KeyStoreSize { get; set; }

        [Value(0, Hidden = true)]
        public string KeyStore { get; set; }

        [Value(1, Hidden = true)]
        public IEnumerable<string> File { get; set; }
    }
}
