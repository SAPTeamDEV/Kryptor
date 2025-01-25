namespace SAPTeam.Kryptor.Tests
{
    public class BackwardCompatibilityTests
    {
        private static readonly byte[] data = Resources.SampleData;
        private static readonly byte[] keyStoreData = Resources.KeyStoreData;
        private static readonly KeyStore ks;

        static BackwardCompatibilityTests() => ks = new KeyStore(keyStoreData);

        private static async Task RunTest(string id, byte[] resource, bool continuous = false, bool removeHash = false, bool dynamicBlockProcessing = false)
        {
            CryptoProviderConfiguration cpc = new CryptoProviderConfiguration()
            {
                Id = id,
                Continuous = continuous,
                RemoveHash = removeHash,
                DynamicBlockProcessing = dynamicBlockProcessing,
            };
            cpc.Id = CryptoProviderFactory.ResolveId(cpc.Id);

            MemoryStream ms = new MemoryStream(data);
            MemoryStream ms2 = new MemoryStream(resource);
            MemoryStream ms3 = new MemoryStream();

            Header header = Header.ReadHeader<Header>(ms2);
            Assert.Equal(cpc, header.Configuration);

            Kes kes = new Kes(ks, header.Configuration);
            await kes.DecryptAsync(ms2, ms3);

            Assert.Equal(ms.ToArray(), ms3.ToArray());
        }

        [Fact]
        public async Task SKTest() => await RunTest("SK", Resources.SK);

        [Fact]
        public async Task TKTest() => await RunTest("TK", Resources.TK);

        [Fact]
        public async Task MVTest() => await RunTest("MV", Resources.MV);

        [Fact]
        public async Task TPTest() => await RunTest("TP", Resources.TP);

        [Fact]
        public async Task DETest() => await RunTest("DE", Resources.DE);

        [Fact]
        public async Task SKWithContinuousTest() => await RunTest("SK", Resources.SKWithContinuous, continuous: true);

        [Fact]
        public async Task TKWithContinuousTest() => await RunTest("TK", Resources.TKWithContinuous, continuous: true);

        [Fact]
        public async Task MVWithContinuousTest() => await RunTest("MV", Resources.MVWithContinuous, continuous: true);

        [Fact]
        public async Task TPWithContinuousTest() => await RunTest("TP", Resources.TPWithContinuous, continuous: true);

        [Fact]
        public async Task DEWithContinuousTest() => await RunTest("DE", Resources.DEWithContinuous, continuous: true);

        [Fact]
        public async Task SKWithRemoveHashTest() => await RunTest("SK", Resources.SKWithRemoveHash, removeHash: true);

        [Fact]
        public async Task MVWithRemoveHashTest() => await RunTest("MV", Resources.MVWithRemoveHash, removeHash: true);

        [Fact]
        public async Task DEWithRemoveHashTest() => await RunTest("DE", Resources.DEWithRemoveHash, removeHash: true);

        [Fact]
        public async Task SKWithDBPTest() => await RunTest("SK", Resources.SKWithDBP, dynamicBlockProcessing: true);

        [Fact]
        public async Task TKWithDBPTest() => await RunTest("TK", Resources.TKWithDBP, dynamicBlockProcessing: true);

        [Fact]
        public async Task MVWithDBPTest() => await RunTest("MV", Resources.MVWithDBP, dynamicBlockProcessing: true);

        [Fact]
        public async Task TPWithDBPTest() => await RunTest("TP", Resources.TPWithDBP, dynamicBlockProcessing: true);

        [Fact]
        public async Task DEWithDBPTest() => await RunTest("DE", Resources.DEWithDBP, dynamicBlockProcessing: true);

        [Fact]
        public async Task SKWithContinuousAndRemoveHashTest() => await RunTest("SK", Resources.SKWithContinuousAndRemoveHash, continuous: true, removeHash: true);

        [Fact]
        public async Task MVWithContinuousAndRemoveHashTest() => await RunTest("MV", Resources.MVWithContinuousAndRemoveHash, continuous: true, removeHash: true);

        [Fact]
        public async Task DEWithContinuousAndRemoveHashTest() => await RunTest("DE", Resources.DEWithContinuousAndRemoveHash, continuous: true, removeHash: true);

        [Fact]
        public async Task SKWithContinuousAndDBPTest() => await RunTest("SK", Resources.SKWithContinuousAndDBP, continuous: true, dynamicBlockProcessing: true);

        [Fact]
        public async Task TKWithContinuousAndDBPTest() => await RunTest("TK", Resources.TKWithContinuousAndDBP, continuous: true, dynamicBlockProcessing: true);

        [Fact]
        public async Task MVWithContinuousAndDBPTest() => await RunTest("MV", Resources.MVWithContinuousAndDBP, continuous: true, dynamicBlockProcessing: true);

        [Fact]
        public async Task TPWithContinuousAndDBPTest() => await RunTest("TP", Resources.TPWithContinuousAndDBP, continuous: true, dynamicBlockProcessing: true);

        [Fact]
        public async Task DEWithContinuousAndDBPTest() => await RunTest("DE", Resources.DEWithContinuousAndDBP, continuous: true, dynamicBlockProcessing: true);

        [Fact]
        public async Task SKWithRemoveHashAndDBPTest() => await RunTest("SK", Resources.SKWithRemoveHashAndDBP, removeHash: true, dynamicBlockProcessing: true);

        [Fact]
        public async Task MVWithRemoveHashAndDBPTest() => await RunTest("MV", Resources.MVWithRemoveHashAndDBP, removeHash: true, dynamicBlockProcessing: true);

        [Fact]
        public async Task DEWithRemoveHashAndDBPTest() => await RunTest("DE", Resources.DEWithRemoveHashAndDBP, removeHash: true, dynamicBlockProcessing: true);

        [Fact]
        public async Task SKWithContinuousAndRemoveHashAndDBPTest() => await RunTest("SK", Resources.SKWithContinuousAndRemoveHashAndDBP, continuous: true, removeHash: true, dynamicBlockProcessing: true);

        [Fact]
        public async Task MVWithContinuousAndRemoveHashAndDBPTest() => await RunTest("MV", Resources.MVWithContinuousAndRemoveHashAndDBP, continuous: true, removeHash: true, dynamicBlockProcessing: true);

        [Fact]
        public async Task DEWithContinuousAndRemoveHashAndDBPTest() => await RunTest("DE", Resources.DEWithContinuousAndRemoveHashAndDBP, continuous: true, removeHash: true, dynamicBlockProcessing: true);
    }
}
