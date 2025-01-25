namespace SAPTeam.Kryptor.Tests
{
    public class DataIntegrityTests
    {
        private static readonly byte[] data = Resources.SampleData;
        private static readonly byte[] keyStoreData = Resources.KeyStoreData;
        private static readonly KeyStore ks;

        static DataIntegrityTests()
        {
            if (!Directory.Exists("testArt"))
            {
                Directory.CreateDirectory("testArt");
            }

            Directory.SetCurrentDirectory("testArt");

            ks = new KeyStore(keyStoreData);
        }

        private static async Task TestEncryptionDecryption(string id, bool continuous = false, bool removeHash = false, bool dynamicBlockProcessing = false)
        {
            CryptoProviderConfiguration cpc = new CryptoProviderConfiguration()
            {
                Id = id,
                Continuous = continuous,
                RemoveHash = removeHash,
                DynamicBlockProcessing = dynamicBlockProcessing,
            };
            Kes kes = new Kes(ks, cpc);

            if (!kes.Provider.IsSecure) return;

            MemoryStream ms = new MemoryStream(data);
            MemoryStream ms2 = new MemoryStream();

            Header header = new Header() { Verbosity = HeaderVerbosity.Maximum, };
            await kes.EncryptAsync(ms, ms2, header);

            File.WriteAllBytes($"{id}{(continuous ? "WithContinuous" : "")}{(removeHash ? "WithRemoveHash" : "")}{(dynamicBlockProcessing ? "WithDBP" : "")}", ms2.ToArray());

            MemoryStream ms3 = new MemoryStream();

            await kes.DecryptAsync(ms2, ms3);
            Assert.Equal(ms.ToArray(), ms3.ToArray());
        }

        [Fact]
        public async Task SKTest() => await TestEncryptionDecryption("SK");

        [Fact]
        public async Task TKTest() => await TestEncryptionDecryption("TK");

        [Fact]
        public async Task MVTest() => await TestEncryptionDecryption("MV");

        [Fact]
        public async Task TPTest() => await TestEncryptionDecryption("TP");

        [Fact]
        public async Task DETest() => await TestEncryptionDecryption("DE");

        [Fact]
        public async Task SKWithContinuousTest() => await TestEncryptionDecryption("SK", continuous: true);

        [Fact]
        public async Task TKWithContinuousTest() => await TestEncryptionDecryption("TK", continuous: true);

        [Fact]
        public async Task MVWithContinuousTest() => await TestEncryptionDecryption("MV", continuous: true);

        [Fact]
        public async Task TPWithContinuousTest() => await TestEncryptionDecryption("TP", continuous: true);

        [Fact]
        public async Task DEWithContinuousTest() => await TestEncryptionDecryption("DE", continuous: true);

        [Fact]
        public async Task SKWithRemoveHashTest() => await TestEncryptionDecryption("SK", removeHash: true);

        [Fact]
        public async Task MVWithRemoveHashTest() => await TestEncryptionDecryption("MV", removeHash: true);

        [Fact]
        public async Task DEWithRemoveHashTest() => await TestEncryptionDecryption("DE", removeHash: true);

        [Fact]
        public async Task SKWithDBPTest() => await TestEncryptionDecryption("SK", dynamicBlockProcessing: true);

        [Fact]
        public async Task TKWithDBPTest() => await TestEncryptionDecryption("TK", dynamicBlockProcessing: true);

        [Fact]
        public async Task MVWithDBPTest() => await TestEncryptionDecryption("MV", dynamicBlockProcessing: true);

        [Fact]
        public async Task TPWithDBPTest() => await TestEncryptionDecryption("TP", dynamicBlockProcessing: true);

        [Fact]
        public async Task DEWithDBPTest() => await TestEncryptionDecryption("DE", dynamicBlockProcessing: true);

        [Fact]
        public async Task SKWithContinuousAndRemoveHashTest() => await TestEncryptionDecryption("SK", continuous: true, removeHash: true);

        [Fact]
        public async Task MVWithContinuousAndRemoveHashTest() => await TestEncryptionDecryption("MV", continuous: true, removeHash: true);

        [Fact]
        public async Task DEWithContinuousAndRemoveHashTest() => await TestEncryptionDecryption("DE", continuous: true, removeHash: true);

        [Fact]
        public async Task SKWithContinuousAndDBPTest() => await TestEncryptionDecryption("SK", continuous: true, dynamicBlockProcessing: true);

        [Fact]
        public async Task TKWithContinuousAndDBPTest() => await TestEncryptionDecryption("TK", continuous: true, dynamicBlockProcessing: true);

        [Fact]
        public async Task MVWithContinuousAndDBPTest() => await TestEncryptionDecryption("MV", continuous: true, dynamicBlockProcessing: true);

        [Fact]
        public async Task TPWithContinuousAndDBPTest() => await TestEncryptionDecryption("TP", continuous: true, dynamicBlockProcessing: true);

        [Fact]
        public async Task DEWithContinuousAndDBPTest() => await TestEncryptionDecryption("DE", continuous: true, dynamicBlockProcessing: true);

        [Fact]
        public async Task SKWithRemoveHashAndDBPTest() => await TestEncryptionDecryption("SK", removeHash: true, dynamicBlockProcessing: true);

        [Fact]
        public async Task MVWithRemoveHashAndDBPTest() => await TestEncryptionDecryption("MV", removeHash: true, dynamicBlockProcessing: true);

        [Fact]
        public async Task DEWithRemoveHashAndDBPTest() => await TestEncryptionDecryption("DE", removeHash: true, dynamicBlockProcessing: true);

        [Fact]
        public async Task SKWithContinuousAndRemoveHashAndDBPTest() => await TestEncryptionDecryption("SK", continuous: true, removeHash: true, dynamicBlockProcessing: true);

        [Fact]
        public async Task MVWithContinuousAndRemoveHashAndDBPTest() => await TestEncryptionDecryption("MV", continuous: true, removeHash: true, dynamicBlockProcessing: true);

        [Fact]
        public async Task DEWithContinuousAndRemoveHashAndDBPTest() => await TestEncryptionDecryption("DE", continuous: true, removeHash: true, dynamicBlockProcessing: true);
    }
}
