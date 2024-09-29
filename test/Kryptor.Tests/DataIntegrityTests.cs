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

        [Fact]
        public void SKTest()
        {
            CryptoProviderConfiguration cpc = new CryptoProviderConfiguration()
            {
                Id = "SK",
            };
            Kes kes = new Kes(ks, cpc);

            if (!kes.Provider.IsSecure) return;

            MemoryStream ms = new MemoryStream(data);
            MemoryStream ms2 = new MemoryStream();

            Header header = new Header() { Verbosity = HeaderVerbosity.Maximum, }; kes.EncryptAsync(ms, ms2, header).Wait();

            File.WriteAllBytes("SK", ms2.ToArray());

            MemoryStream ms3 = new MemoryStream();

            kes.DecryptAsync(ms2, ms3).Wait();
            Assert.Equal(ms.ToArray(), ms3.ToArray());
        }

        [Fact]
        public void TKTest()
        {
            CryptoProviderConfiguration cpc = new CryptoProviderConfiguration()
            {
                Id = "TK",
            };
            Kes kes = new Kes(ks, cpc);

            if (!kes.Provider.IsSecure) return;

            MemoryStream ms = new MemoryStream(data);
            MemoryStream ms2 = new MemoryStream();

            Header header = new Header() { Verbosity = HeaderVerbosity.Maximum, }; kes.EncryptAsync(ms, ms2, header).Wait();

            File.WriteAllBytes("TK", ms2.ToArray());

            MemoryStream ms3 = new MemoryStream();

            kes.DecryptAsync(ms2, ms3).Wait();
            Assert.Equal(ms.ToArray(), ms3.ToArray());
        }

        [Fact]
        public void MVTest()
        {
            CryptoProviderConfiguration cpc = new CryptoProviderConfiguration()
            {
                Id = "MV",
            };
            Kes kes = new Kes(ks, cpc);

            MemoryStream ms = new MemoryStream(data);
            MemoryStream ms2 = new MemoryStream();

            Header header = new Header() { Verbosity = HeaderVerbosity.Maximum, }; kes.EncryptAsync(ms, ms2, header).Wait();

            File.WriteAllBytes("MV", ms2.ToArray());

            MemoryStream ms3 = new MemoryStream();

            kes.DecryptAsync(ms2, ms3).Wait();
            Assert.Equal(ms.ToArray(), ms3.ToArray());
        }

        [Fact]
        public void TPTest()
        {
            CryptoProviderConfiguration cpc = new CryptoProviderConfiguration()
            {
                Id = "TP",
            };
            Kes kes = new Kes(ks, cpc);

            MemoryStream ms = new MemoryStream(data);
            MemoryStream ms2 = new MemoryStream();

            Header header = new Header() { Verbosity = HeaderVerbosity.Maximum, }; kes.EncryptAsync(ms, ms2, header).Wait();

            File.WriteAllBytes("TP", ms2.ToArray());

            MemoryStream ms3 = new MemoryStream();

            kes.DecryptAsync(ms2, ms3).Wait();
            Assert.Equal(ms.ToArray(), ms3.ToArray());
        }

        [Fact]
        public void DETest()
        {
            CryptoProviderConfiguration cpc = new CryptoProviderConfiguration()
            {
                Id = "DE",
            };
            Kes kes = new Kes(ks, cpc);

            MemoryStream ms = new MemoryStream(data);
            MemoryStream ms2 = new MemoryStream();

            Header header = new Header() { Verbosity = HeaderVerbosity.Maximum, }; kes.EncryptAsync(ms, ms2, header).Wait();

            File.WriteAllBytes("DE", ms2.ToArray());

            MemoryStream ms3 = new MemoryStream();

            kes.DecryptAsync(ms2, ms3).Wait();
            Assert.Equal(ms.ToArray(), ms3.ToArray());
        }

        [Fact]
        public void SKWithContinuousTest()
        {
            CryptoProviderConfiguration cpc = new CryptoProviderConfiguration()
            {
                Id = "SK",
                Continuous = true,
            };
            Kes kes = new Kes(ks, cpc);

            if (!kes.Provider.IsSecure) return;

            MemoryStream ms = new MemoryStream(data);
            MemoryStream ms2 = new MemoryStream();

            Header header = new Header() { Verbosity = HeaderVerbosity.Maximum, }; kes.EncryptAsync(ms, ms2, header).Wait();

            File.WriteAllBytes("SKWithContinuous", ms2.ToArray());

            MemoryStream ms3 = new MemoryStream();

            kes.DecryptAsync(ms2, ms3).Wait();
            Assert.Equal(ms.ToArray(), ms3.ToArray());
        }

        [Fact]
        public void TKWithContinuousTest()
        {
            CryptoProviderConfiguration cpc = new CryptoProviderConfiguration()
            {
                Id = "TK",
                Continuous = true,
            };
            Kes kes = new Kes(ks, cpc);

            if (!kes.Provider.IsSecure) return;

            MemoryStream ms = new MemoryStream(data);
            MemoryStream ms2 = new MemoryStream();

            Header header = new Header() { Verbosity = HeaderVerbosity.Maximum, }; kes.EncryptAsync(ms, ms2, header).Wait();

            File.WriteAllBytes("TKWithContinuous", ms2.ToArray());

            MemoryStream ms3 = new MemoryStream();

            kes.DecryptAsync(ms2, ms3).Wait();
            Assert.Equal(ms.ToArray(), ms3.ToArray());
        }

        [Fact]
        public void MVWithContinuousTest()
        {
            CryptoProviderConfiguration cpc = new CryptoProviderConfiguration()
            {
                Id = "MV",
                Continuous = true,
            };
            Kes kes = new Kes(ks, cpc);

            MemoryStream ms = new MemoryStream(data);
            MemoryStream ms2 = new MemoryStream();

            Header header = new Header() { Verbosity = HeaderVerbosity.Maximum, }; kes.EncryptAsync(ms, ms2, header).Wait();

            File.WriteAllBytes("MVWithContinuous", ms2.ToArray());

            MemoryStream ms3 = new MemoryStream();

            kes.DecryptAsync(ms2, ms3).Wait();
            Assert.Equal(ms.ToArray(), ms3.ToArray());
        }

        [Fact]
        public void TPWithContinuousTest()
        {
            CryptoProviderConfiguration cpc = new CryptoProviderConfiguration()
            {
                Id = "TP",
                Continuous = true,
            };
            Kes kes = new Kes(ks, cpc);

            MemoryStream ms = new MemoryStream(data);
            MemoryStream ms2 = new MemoryStream();

            Header header = new Header() { Verbosity = HeaderVerbosity.Maximum, }; kes.EncryptAsync(ms, ms2, header).Wait();

            File.WriteAllBytes("TPWithContinuous", ms2.ToArray());

            MemoryStream ms3 = new MemoryStream();

            kes.DecryptAsync(ms2, ms3).Wait();
            Assert.Equal(ms.ToArray(), ms3.ToArray());
        }

        [Fact]
        public void DEWithContinuousTest()
        {
            CryptoProviderConfiguration cpc = new CryptoProviderConfiguration()
            {
                Id = "DE",
                Continuous = true,
            };
            Kes kes = new Kes(ks, cpc);

            MemoryStream ms = new MemoryStream(data);
            MemoryStream ms2 = new MemoryStream();

            Header header = new Header() { Verbosity = HeaderVerbosity.Maximum, }; kes.EncryptAsync(ms, ms2, header).Wait();

            File.WriteAllBytes("DEWithContinuous", ms2.ToArray());

            MemoryStream ms3 = new MemoryStream();

            kes.DecryptAsync(ms2, ms3).Wait();
            Assert.Equal(ms.ToArray(), ms3.ToArray());
        }

        [Fact]
        public void SKWithRemoveHashTest()
        {
            CryptoProviderConfiguration cpc = new CryptoProviderConfiguration()
            {
                Id = "SK",
                RemoveHash = true,
            };
            Kes kes = new Kes(ks, cpc);

            if (!kes.Provider.IsSecure) return;

            MemoryStream ms = new MemoryStream(data);
            MemoryStream ms2 = new MemoryStream();

            Header header = new Header() { Verbosity = HeaderVerbosity.Maximum, }; kes.EncryptAsync(ms, ms2, header).Wait();

            File.WriteAllBytes("SKWithRemoveHash", ms2.ToArray());

            MemoryStream ms3 = new MemoryStream();

            kes.DecryptAsync(ms2, ms3).Wait();
            Assert.Equal(ms.ToArray(), ms3.ToArray());
        }

        [Fact]
        public void MVWithRemoveHashTest()
        {
            CryptoProviderConfiguration cpc = new CryptoProviderConfiguration()
            {
                Id = "MV",
                RemoveHash = true,
            };
            Kes kes = new Kes(ks, cpc);

            MemoryStream ms = new MemoryStream(data);
            MemoryStream ms2 = new MemoryStream();

            Header header = new Header() { Verbosity = HeaderVerbosity.Maximum, }; kes.EncryptAsync(ms, ms2, header).Wait();

            File.WriteAllBytes("MVWithRemoveHash", ms2.ToArray());

            MemoryStream ms3 = new MemoryStream();

            kes.DecryptAsync(ms2, ms3).Wait();
            Assert.Equal(ms.ToArray(), ms3.ToArray());
        }

        [Fact]
        public void DEWithRemoveHashTest()
        {
            CryptoProviderConfiguration cpc = new CryptoProviderConfiguration()
            {
                Id = "DE",
                RemoveHash = true,
            };
            Kes kes = new Kes(ks, cpc);

            MemoryStream ms = new MemoryStream(data);
            MemoryStream ms2 = new MemoryStream();

            Header header = new Header() { Verbosity = HeaderVerbosity.Maximum, }; kes.EncryptAsync(ms, ms2, header).Wait();

            File.WriteAllBytes("DEWithRemoveHash", ms2.ToArray());

            MemoryStream ms3 = new MemoryStream();

            kes.DecryptAsync(ms2, ms3).Wait();
            Assert.Equal(ms.ToArray(), ms3.ToArray());
        }

        [Fact]
        public void SKWithDBPTest()
        {
            CryptoProviderConfiguration cpc = new CryptoProviderConfiguration()
            {
                Id = "SK",
                DynamicBlockProccessing = true,
            };
            Kes kes = new Kes(ks, cpc);

            if (!kes.Provider.IsSecure) return;

            MemoryStream ms = new MemoryStream(data);
            MemoryStream ms2 = new MemoryStream();

            Header header = new Header() { Verbosity = HeaderVerbosity.Maximum, }; kes.EncryptAsync(ms, ms2, header).Wait();

            File.WriteAllBytes("SKWithDBP", ms2.ToArray());

            MemoryStream ms3 = new MemoryStream();

            kes.DecryptAsync(ms2, ms3).Wait();
            Assert.Equal(ms.ToArray(), ms3.ToArray());
        }

        [Fact]
        public void TKWithDBPTest()
        {
            CryptoProviderConfiguration cpc = new CryptoProviderConfiguration()
            {
                Id = "TK",
                DynamicBlockProccessing = true,
            };
            Kes kes = new Kes(ks, cpc);

            if (!kes.Provider.IsSecure) return;

            MemoryStream ms = new MemoryStream(data);
            MemoryStream ms2 = new MemoryStream();

            Header header = new Header() { Verbosity = HeaderVerbosity.Maximum, }; kes.EncryptAsync(ms, ms2, header).Wait();

            File.WriteAllBytes("TKWithDBP", ms2.ToArray());

            MemoryStream ms3 = new MemoryStream();

            kes.DecryptAsync(ms2, ms3).Wait();
            Assert.Equal(ms.ToArray(), ms3.ToArray());
        }

        [Fact]
        public void MVWithDBPTest()
        {
            CryptoProviderConfiguration cpc = new CryptoProviderConfiguration()
            {
                Id = "MV",
                DynamicBlockProccessing = true,
            };
            Kes kes = new Kes(ks, cpc);

            MemoryStream ms = new MemoryStream(data);
            MemoryStream ms2 = new MemoryStream();

            Header header = new Header() { Verbosity = HeaderVerbosity.Maximum, }; kes.EncryptAsync(ms, ms2, header).Wait();

            File.WriteAllBytes("MVWithDBP", ms2.ToArray());

            MemoryStream ms3 = new MemoryStream();

            kes.DecryptAsync(ms2, ms3).Wait();
            Assert.Equal(ms.ToArray(), ms3.ToArray());
        }

        [Fact]
        public void TPWithDBPTest()
        {
            CryptoProviderConfiguration cpc = new CryptoProviderConfiguration()
            {
                Id = "TP",
                DynamicBlockProccessing = true,
            };
            Kes kes = new Kes(ks, cpc);

            MemoryStream ms = new MemoryStream(data);
            MemoryStream ms2 = new MemoryStream();

            Header header = new Header() { Verbosity = HeaderVerbosity.Maximum, }; kes.EncryptAsync(ms, ms2, header).Wait();

            File.WriteAllBytes("TPWithDBP", ms2.ToArray());

            MemoryStream ms3 = new MemoryStream();

            kes.DecryptAsync(ms2, ms3).Wait();
            Assert.Equal(ms.ToArray(), ms3.ToArray());
        }

        [Fact]
        public void DEWithDBPTest()
        {
            CryptoProviderConfiguration cpc = new CryptoProviderConfiguration()
            {
                Id = "DE",
                DynamicBlockProccessing = true,
            };
            Kes kes = new Kes(ks, cpc);

            MemoryStream ms = new MemoryStream(data);
            MemoryStream ms2 = new MemoryStream();

            Header header = new Header() { Verbosity = HeaderVerbosity.Maximum, }; kes.EncryptAsync(ms, ms2, header).Wait();

            File.WriteAllBytes("DEWithDBP", ms2.ToArray());

            MemoryStream ms3 = new MemoryStream();

            kes.DecryptAsync(ms2, ms3).Wait();
            Assert.Equal(ms.ToArray(), ms3.ToArray());
        }

        [Fact]
        public void SKWithContinuousAndRemoveHashTest()
        {
            CryptoProviderConfiguration cpc = new CryptoProviderConfiguration()
            {
                Id = "SK",
                Continuous = true,
                RemoveHash = true,
            };
            Kes kes = new Kes(ks, cpc);

            if (!kes.Provider.IsSecure) return;

            MemoryStream ms = new MemoryStream(data);
            MemoryStream ms2 = new MemoryStream();

            Header header = new Header() { Verbosity = HeaderVerbosity.Maximum, }; kes.EncryptAsync(ms, ms2, header).Wait();

            File.WriteAllBytes("SKWithContinuousAndRemoveHash", ms2.ToArray());

            MemoryStream ms3 = new MemoryStream();

            kes.DecryptAsync(ms2, ms3).Wait();
            Assert.Equal(ms.ToArray(), ms3.ToArray());
        }

        [Fact]
        public void MVWithContinuousAndRemoveHashTest()
        {
            CryptoProviderConfiguration cpc = new CryptoProviderConfiguration()
            {
                Id = "MV",
                Continuous = true,
                RemoveHash = true,
            };
            Kes kes = new Kes(ks, cpc);

            MemoryStream ms = new MemoryStream(data);
            MemoryStream ms2 = new MemoryStream();

            Header header = new Header() { Verbosity = HeaderVerbosity.Maximum, }; kes.EncryptAsync(ms, ms2, header).Wait();

            File.WriteAllBytes("MVWithContinuousAndRemoveHash", ms2.ToArray());

            MemoryStream ms3 = new MemoryStream();

            kes.DecryptAsync(ms2, ms3).Wait();
            Assert.Equal(ms.ToArray(), ms3.ToArray());
        }

        [Fact]
        public void DEWithContinuousAndRemoveHashTest()
        {
            CryptoProviderConfiguration cpc = new CryptoProviderConfiguration()
            {
                Id = "DE",
                Continuous = true,
                RemoveHash = true,
            };
            Kes kes = new Kes(ks, cpc);

            MemoryStream ms = new MemoryStream(data);
            MemoryStream ms2 = new MemoryStream();

            Header header = new Header() { Verbosity = HeaderVerbosity.Maximum, }; kes.EncryptAsync(ms, ms2, header).Wait();

            File.WriteAllBytes("DEWithContinuousAndRemoveHash", ms2.ToArray());

            MemoryStream ms3 = new MemoryStream();

            kes.DecryptAsync(ms2, ms3).Wait();
            Assert.Equal(ms.ToArray(), ms3.ToArray());
        }

        [Fact]
        public void SKWithContinuousAndDBPTest()
        {
            CryptoProviderConfiguration cpc = new CryptoProviderConfiguration()
            {
                Id = "SK",
                Continuous = true,
                DynamicBlockProccessing = true,
            };
            Kes kes = new Kes(ks, cpc);

            if (!kes.Provider.IsSecure) return;

            MemoryStream ms = new MemoryStream(data);
            MemoryStream ms2 = new MemoryStream();

            Header header = new Header() { Verbosity = HeaderVerbosity.Maximum, }; kes.EncryptAsync(ms, ms2, header).Wait();

            File.WriteAllBytes("SKWithContinuousAndDBP", ms2.ToArray());

            MemoryStream ms3 = new MemoryStream();

            kes.DecryptAsync(ms2, ms3).Wait();
            Assert.Equal(ms.ToArray(), ms3.ToArray());
        }

        [Fact]
        public void TKWithContinuousAndDBPTest()
        {
            CryptoProviderConfiguration cpc = new CryptoProviderConfiguration()
            {
                Id = "TK",
                Continuous = true,
                DynamicBlockProccessing = true,
            };
            Kes kes = new Kes(ks, cpc);

            if (!kes.Provider.IsSecure) return;

            MemoryStream ms = new MemoryStream(data);
            MemoryStream ms2 = new MemoryStream();

            Header header = new Header() { Verbosity = HeaderVerbosity.Maximum, }; kes.EncryptAsync(ms, ms2, header).Wait();

            File.WriteAllBytes("TKWithContinuousAndDBP", ms2.ToArray());

            MemoryStream ms3 = new MemoryStream();

            kes.DecryptAsync(ms2, ms3).Wait();
            Assert.Equal(ms.ToArray(), ms3.ToArray());
        }

        [Fact]
        public void MVWithContinuousAndDBPTest()
        {
            CryptoProviderConfiguration cpc = new CryptoProviderConfiguration()
            {
                Id = "MV",
                Continuous = true,
                DynamicBlockProccessing = true,
            };
            Kes kes = new Kes(ks, cpc);

            MemoryStream ms = new MemoryStream(data);
            MemoryStream ms2 = new MemoryStream();

            Header header = new Header() { Verbosity = HeaderVerbosity.Maximum, }; kes.EncryptAsync(ms, ms2, header).Wait();

            File.WriteAllBytes("MVWithContinuousAndDBP", ms2.ToArray());

            MemoryStream ms3 = new MemoryStream();

            kes.DecryptAsync(ms2, ms3).Wait();
            Assert.Equal(ms.ToArray(), ms3.ToArray());
        }

        [Fact]
        public void TPWithContinuousAndDBPTest()
        {
            CryptoProviderConfiguration cpc = new CryptoProviderConfiguration()
            {
                Id = "TP",
                Continuous = true,
                DynamicBlockProccessing = true,
            };
            Kes kes = new Kes(ks, cpc);

            MemoryStream ms = new MemoryStream(data);
            MemoryStream ms2 = new MemoryStream();

            Header header = new Header() { Verbosity = HeaderVerbosity.Maximum, }; kes.EncryptAsync(ms, ms2, header).Wait();

            File.WriteAllBytes("TPWithContinuousAndDBP", ms2.ToArray());

            MemoryStream ms3 = new MemoryStream();

            kes.DecryptAsync(ms2, ms3).Wait();
            Assert.Equal(ms.ToArray(), ms3.ToArray());
        }

        [Fact]
        public void DEWithContinuousAndDBPTest()
        {
            CryptoProviderConfiguration cpc = new CryptoProviderConfiguration()
            {
                Id = "DE",
                Continuous = true,
                DynamicBlockProccessing = true,
            };
            Kes kes = new Kes(ks, cpc);

            MemoryStream ms = new MemoryStream(data);
            MemoryStream ms2 = new MemoryStream();

            Header header = new Header() { Verbosity = HeaderVerbosity.Maximum, }; kes.EncryptAsync(ms, ms2, header).Wait();

            File.WriteAllBytes("DEWithContinuousAndDBP", ms2.ToArray());

            MemoryStream ms3 = new MemoryStream();

            kes.DecryptAsync(ms2, ms3).Wait();
            Assert.Equal(ms.ToArray(), ms3.ToArray());
        }

        [Fact]
        public void SKWithRemoveHashAndDBPTest()
        {
            CryptoProviderConfiguration cpc = new CryptoProviderConfiguration()
            {
                Id = "SK",
                RemoveHash = true,
                DynamicBlockProccessing = true,
            };
            Kes kes = new Kes(ks, cpc);

            if (!kes.Provider.IsSecure) return;

            MemoryStream ms = new MemoryStream(data);
            MemoryStream ms2 = new MemoryStream();

            Header header = new Header() { Verbosity = HeaderVerbosity.Maximum, }; kes.EncryptAsync(ms, ms2, header).Wait();

            File.WriteAllBytes("SKWithRemoveHashAndDBP", ms2.ToArray());

            MemoryStream ms3 = new MemoryStream();

            kes.DecryptAsync(ms2, ms3).Wait();
            Assert.Equal(ms.ToArray(), ms3.ToArray());
        }

        [Fact]
        public void MVWithRemoveHashAndDBPTest()
        {
            CryptoProviderConfiguration cpc = new CryptoProviderConfiguration()
            {
                Id = "MV",
                RemoveHash = true,
                DynamicBlockProccessing = true,
            };
            Kes kes = new Kes(ks, cpc);

            MemoryStream ms = new MemoryStream(data);
            MemoryStream ms2 = new MemoryStream();

            Header header = new Header() { Verbosity = HeaderVerbosity.Maximum, }; kes.EncryptAsync(ms, ms2, header).Wait();

            File.WriteAllBytes("MVWithRemoveHashAndDBP", ms2.ToArray());

            MemoryStream ms3 = new MemoryStream();

            kes.DecryptAsync(ms2, ms3).Wait();
            Assert.Equal(ms.ToArray(), ms3.ToArray());
        }

        [Fact]
        public void DEWithRemoveHashAndDBPTest()
        {
            CryptoProviderConfiguration cpc = new CryptoProviderConfiguration()
            {
                Id = "DE",
                RemoveHash = true,
                DynamicBlockProccessing = true,
            };
            Kes kes = new Kes(ks, cpc);

            MemoryStream ms = new MemoryStream(data);
            MemoryStream ms2 = new MemoryStream();

            Header header = new Header() { Verbosity = HeaderVerbosity.Maximum, }; kes.EncryptAsync(ms, ms2, header).Wait();

            File.WriteAllBytes("DEWithRemoveHashAndDBP", ms2.ToArray());

            MemoryStream ms3 = new MemoryStream();

            kes.DecryptAsync(ms2, ms3).Wait();
            Assert.Equal(ms.ToArray(), ms3.ToArray());
        }

        [Fact]
        public void SKWithContinuousAndRemoveHashAndDBPTest()
        {
            CryptoProviderConfiguration cpc = new CryptoProviderConfiguration()
            {
                Id = "SK",
                Continuous = true,
                RemoveHash = true,
                DynamicBlockProccessing = true,
            };
            Kes kes = new Kes(ks, cpc);

            if (!kes.Provider.IsSecure) return;

            MemoryStream ms = new MemoryStream(data);
            MemoryStream ms2 = new MemoryStream();

            Header header = new Header() { Verbosity = HeaderVerbosity.Maximum, }; kes.EncryptAsync(ms, ms2, header).Wait();

            File.WriteAllBytes("SKWithContinuousAndRemoveHashAndDBP", ms2.ToArray());

            MemoryStream ms3 = new MemoryStream();

            kes.DecryptAsync(ms2, ms3).Wait();
            Assert.Equal(ms.ToArray(), ms3.ToArray());
        }

        [Fact]
        public void MVWithContinuousAndRemoveHashAndDBPTest()
        {
            CryptoProviderConfiguration cpc = new CryptoProviderConfiguration()
            {
                Id = "MV",
                Continuous = true,
                RemoveHash = true,
                DynamicBlockProccessing = true,
            };
            Kes kes = new Kes(ks, cpc);

            MemoryStream ms = new MemoryStream(data);
            MemoryStream ms2 = new MemoryStream();

            Header header = new Header() { Verbosity = HeaderVerbosity.Maximum, }; kes.EncryptAsync(ms, ms2, header).Wait();

            File.WriteAllBytes("MVWithContinuousAndRemoveHashAndDBP", ms2.ToArray());

            MemoryStream ms3 = new MemoryStream();

            kes.DecryptAsync(ms2, ms3).Wait();
            Assert.Equal(ms.ToArray(), ms3.ToArray());
        }

        [Fact]
        public void DEWithContinuousAndRemoveHashAndDBPTest()
        {
            CryptoProviderConfiguration cpc = new CryptoProviderConfiguration()
            {
                Id = "DE",
                Continuous = true,
                RemoveHash = true,
                DynamicBlockProccessing = true,
            };
            Kes kes = new Kes(ks, cpc);

            MemoryStream ms = new MemoryStream(data);
            MemoryStream ms2 = new MemoryStream();

            Header header = new Header() { Verbosity = HeaderVerbosity.Maximum, }; kes.EncryptAsync(ms, ms2, header).Wait();

            File.WriteAllBytes("DEWithContinuousAndRemoveHashAndDBP", ms2.ToArray());

            MemoryStream ms3 = new MemoryStream();

            kes.DecryptAsync(ms2, ms3).Wait();
            Assert.Equal(ms.ToArray(), ms3.ToArray());
        }
    }
}
