using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SAPTeam.Kryptor.CryptoProviders;

namespace SAPTeam.Kryptor.Tests
{
    public class BackwardCompatibilityTests
    {
        private readonly static byte[] data = Resources.SampleData;
        private readonly static byte[] keyStoreData = Resources.KeyStoreData;
        private readonly static KeyStore ks;

        static BackwardCompatibilityTests()
        {
            ks = new KeyStore(keyStoreData);
        }

        [Fact]
        public void SKTest()
        {
            CryptoProviderConfiguration cpc = new CryptoProviderConfiguration()
            {
                Id = "SK",
            };
            cpc.Id = CryptoProviderFactory.GetRegisteredCryptoProviderId(cpc.Id);

            MemoryStream ms = new MemoryStream(data);
            MemoryStream ms2 = new MemoryStream(Resources.SK);
            MemoryStream ms3 = new MemoryStream();

            var header = Header.ReadHeader<Header>(ms2);
            Assert.Equal(cpc, header.Configuration);

            Kes kes = new Kes(ks, header.Configuration);
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
            cpc.Id = CryptoProviderFactory.GetRegisteredCryptoProviderId(cpc.Id);

            MemoryStream ms = new MemoryStream(data);
            MemoryStream ms2 = new MemoryStream(Resources.TK);
            MemoryStream ms3 = new MemoryStream();

            var header = Header.ReadHeader<Header>(ms2);
            Assert.Equal(cpc, header.Configuration);

            Kes kes = new Kes(ks, header.Configuration);
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
            cpc.Id = CryptoProviderFactory.GetRegisteredCryptoProviderId(cpc.Id);

            MemoryStream ms = new MemoryStream(data);
            MemoryStream ms2 = new MemoryStream(Resources.MV);
            MemoryStream ms3 = new MemoryStream();

            var header = Header.ReadHeader<Header>(ms2);
            Assert.Equal(cpc, header.Configuration);

            Kes kes = new Kes(ks, header.Configuration);
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
            cpc.Id = CryptoProviderFactory.GetRegisteredCryptoProviderId(cpc.Id);

            MemoryStream ms = new MemoryStream(data);
            MemoryStream ms2 = new MemoryStream(Resources.TP);
            MemoryStream ms3 = new MemoryStream();

            var header = Header.ReadHeader<Header>(ms2);
            Assert.Equal(cpc, header.Configuration);

            Kes kes = new Kes(ks, header.Configuration);
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
            cpc.Id = CryptoProviderFactory.GetRegisteredCryptoProviderId(cpc.Id);

            MemoryStream ms = new MemoryStream(data);
            MemoryStream ms2 = new MemoryStream(Resources.DE);
            MemoryStream ms3 = new MemoryStream();

            var header = Header.ReadHeader<Header>(ms2);
            Assert.Equal(cpc, header.Configuration);

            Kes kes = new Kes(ks, header.Configuration);
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
            cpc.Id = CryptoProviderFactory.GetRegisteredCryptoProviderId(cpc.Id);

            MemoryStream ms = new MemoryStream(data);
            MemoryStream ms2 = new MemoryStream(Resources.SKWithContinuous);
            MemoryStream ms3 = new MemoryStream();

            var header = Header.ReadHeader<Header>(ms2);
            Assert.Equal(cpc, header.Configuration);

            Kes kes = new Kes(ks, header.Configuration);
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
            cpc.Id = CryptoProviderFactory.GetRegisteredCryptoProviderId(cpc.Id);

            MemoryStream ms = new MemoryStream(data);
            MemoryStream ms2 = new MemoryStream(Resources.TKWithContinuous);
            MemoryStream ms3 = new MemoryStream();

            var header = Header.ReadHeader<Header>(ms2);
            Assert.Equal(cpc, header.Configuration);

            Kes kes = new Kes(ks, header.Configuration);
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
            cpc.Id = CryptoProviderFactory.GetRegisteredCryptoProviderId(cpc.Id);

            MemoryStream ms = new MemoryStream(data);
            MemoryStream ms2 = new MemoryStream(Resources.MVWithContinuous);
            MemoryStream ms3 = new MemoryStream();

            var header = Header.ReadHeader<Header>(ms2);
            Assert.Equal(cpc, header.Configuration);

            Kes kes = new Kes(ks, header.Configuration);
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
            cpc.Id = CryptoProviderFactory.GetRegisteredCryptoProviderId(cpc.Id);

            MemoryStream ms = new MemoryStream(data);
            MemoryStream ms2 = new MemoryStream(Resources.TPWithContinuous);
            MemoryStream ms3 = new MemoryStream();

            var header = Header.ReadHeader<Header>(ms2);
            Assert.Equal(cpc, header.Configuration);

            Kes kes = new Kes(ks, header.Configuration);
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
            cpc.Id = CryptoProviderFactory.GetRegisteredCryptoProviderId(cpc.Id);

            MemoryStream ms = new MemoryStream(data);
            MemoryStream ms2 = new MemoryStream(Resources.DEWithContinuous);
            MemoryStream ms3 = new MemoryStream();

            var header = Header.ReadHeader<Header>(ms2);
            Assert.Equal(cpc, header.Configuration);

            Kes kes = new Kes(ks, header.Configuration);
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
            cpc.Id = CryptoProviderFactory.GetRegisteredCryptoProviderId(cpc.Id);

            MemoryStream ms = new MemoryStream(data);
            MemoryStream ms2 = new MemoryStream(Resources.SKWithRemoveHash);
            MemoryStream ms3 = new MemoryStream();

            var header = Header.ReadHeader<Header>(ms2);
            Assert.Equal(cpc, header.Configuration);

            Kes kes = new Kes(ks, header.Configuration);
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
            cpc.Id = CryptoProviderFactory.GetRegisteredCryptoProviderId(cpc.Id);

            MemoryStream ms = new MemoryStream(data);
            MemoryStream ms2 = new MemoryStream(Resources.MVWithRemoveHash);
            MemoryStream ms3 = new MemoryStream();

            var header = Header.ReadHeader<Header>(ms2);
            Assert.Equal(cpc, header.Configuration);

            Kes kes = new Kes(ks, header.Configuration);
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
            cpc.Id = CryptoProviderFactory.GetRegisteredCryptoProviderId(cpc.Id);

            MemoryStream ms = new MemoryStream(data);
            MemoryStream ms2 = new MemoryStream(Resources.DEWithRemoveHash);
            MemoryStream ms3 = new MemoryStream();

            var header = Header.ReadHeader<Header>(ms2);
            Assert.Equal(cpc, header.Configuration);

            Kes kes = new Kes(ks, header.Configuration);
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
            cpc.Id = CryptoProviderFactory.GetRegisteredCryptoProviderId(cpc.Id);

            MemoryStream ms = new MemoryStream(data);
            MemoryStream ms2 = new MemoryStream(Resources.SKWithDBP);
            MemoryStream ms3 = new MemoryStream();

            var header = Header.ReadHeader<Header>(ms2);
            Assert.Equal(cpc, header.Configuration);

            Kes kes = new Kes(ks, header.Configuration);
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
            cpc.Id = CryptoProviderFactory.GetRegisteredCryptoProviderId(cpc.Id);

            MemoryStream ms = new MemoryStream(data);
            MemoryStream ms2 = new MemoryStream(Resources.TKWithDBP);
            MemoryStream ms3 = new MemoryStream();

            var header = Header.ReadHeader<Header>(ms2);
            Assert.Equal(cpc, header.Configuration);

            Kes kes = new Kes(ks, header.Configuration);
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
            cpc.Id = CryptoProviderFactory.GetRegisteredCryptoProviderId(cpc.Id);

            MemoryStream ms = new MemoryStream(data);
            MemoryStream ms2 = new MemoryStream(Resources.MVWithDBP);
            MemoryStream ms3 = new MemoryStream();

            var header = Header.ReadHeader<Header>(ms2);
            Assert.Equal(cpc, header.Configuration);

            Kes kes = new Kes(ks, header.Configuration);
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
            cpc.Id = CryptoProviderFactory.GetRegisteredCryptoProviderId(cpc.Id);

            MemoryStream ms = new MemoryStream(data);
            MemoryStream ms2 = new MemoryStream(Resources.TPWithDBP);
            MemoryStream ms3 = new MemoryStream();

            var header = Header.ReadHeader<Header>(ms2);
            Assert.Equal(cpc, header.Configuration);

            Kes kes = new Kes(ks, header.Configuration);
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
            cpc.Id = CryptoProviderFactory.GetRegisteredCryptoProviderId(cpc.Id);

            MemoryStream ms = new MemoryStream(data);
            MemoryStream ms2 = new MemoryStream(Resources.DEWithDBP);
            MemoryStream ms3 = new MemoryStream();

            var header = Header.ReadHeader<Header>(ms2);
            Assert.Equal(cpc, header.Configuration);

            Kes kes = new Kes(ks, header.Configuration);
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
            cpc.Id = CryptoProviderFactory.GetRegisteredCryptoProviderId(cpc.Id);

            MemoryStream ms = new MemoryStream(data);
            MemoryStream ms2 = new MemoryStream(Resources.SKWithContinuousAndRemoveHash);
            MemoryStream ms3 = new MemoryStream();

            var header = Header.ReadHeader<Header>(ms2);
            Assert.Equal(cpc, header.Configuration);

            Kes kes = new Kes(ks, header.Configuration);
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
            cpc.Id = CryptoProviderFactory.GetRegisteredCryptoProviderId(cpc.Id);

            MemoryStream ms = new MemoryStream(data);
            MemoryStream ms2 = new MemoryStream(Resources.MVWithContinuousAndRemoveHash);
            MemoryStream ms3 = new MemoryStream();

            var header = Header.ReadHeader<Header>(ms2);
            Assert.Equal(cpc, header.Configuration);

            Kes kes = new Kes(ks, header.Configuration);
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
            cpc.Id = CryptoProviderFactory.GetRegisteredCryptoProviderId(cpc.Id);

            MemoryStream ms = new MemoryStream(data);
            MemoryStream ms2 = new MemoryStream(Resources.DEWithContinuousAndRemoveHash);
            MemoryStream ms3 = new MemoryStream();

            var header = Header.ReadHeader<Header>(ms2);
            Assert.Equal(cpc, header.Configuration);

            Kes kes = new Kes(ks, header.Configuration);
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
            cpc.Id = CryptoProviderFactory.GetRegisteredCryptoProviderId(cpc.Id);

            MemoryStream ms = new MemoryStream(data);
            MemoryStream ms2 = new MemoryStream(Resources.SKWithContinuousAndDBP);
            MemoryStream ms3 = new MemoryStream();

            var header = Header.ReadHeader<Header>(ms2);
            Assert.Equal(cpc, header.Configuration);

            Kes kes = new Kes(ks, header.Configuration);
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
            cpc.Id = CryptoProviderFactory.GetRegisteredCryptoProviderId(cpc.Id);

            MemoryStream ms = new MemoryStream(data);
            MemoryStream ms2 = new MemoryStream(Resources.TKWithContinuousAndDBP);
            MemoryStream ms3 = new MemoryStream();

            var header = Header.ReadHeader<Header>(ms2);
            Assert.Equal(cpc, header.Configuration);

            Kes kes = new Kes(ks, header.Configuration);
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
            cpc.Id = CryptoProviderFactory.GetRegisteredCryptoProviderId(cpc.Id);

            MemoryStream ms = new MemoryStream(data);
            MemoryStream ms2 = new MemoryStream(Resources.MVWithContinuousAndDBP);
            MemoryStream ms3 = new MemoryStream();

            var header = Header.ReadHeader<Header>(ms2);
            Assert.Equal(cpc, header.Configuration);

            Kes kes = new Kes(ks, header.Configuration);
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
            cpc.Id = CryptoProviderFactory.GetRegisteredCryptoProviderId(cpc.Id);

            MemoryStream ms = new MemoryStream(data);
            MemoryStream ms2 = new MemoryStream(Resources.TPWithContinuousAndDBP);
            MemoryStream ms3 = new MemoryStream();

            var header = Header.ReadHeader<Header>(ms2);
            Assert.Equal(cpc, header.Configuration);

            Kes kes = new Kes(ks, header.Configuration);
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
            cpc.Id = CryptoProviderFactory.GetRegisteredCryptoProviderId(cpc.Id);

            MemoryStream ms = new MemoryStream(data);
            MemoryStream ms2 = new MemoryStream(Resources.DEWithContinuousAndDBP);
            MemoryStream ms3 = new MemoryStream();

            var header = Header.ReadHeader<Header>(ms2);
            Assert.Equal(cpc, header.Configuration);

            Kes kes = new Kes(ks, header.Configuration);
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
            cpc.Id = CryptoProviderFactory.GetRegisteredCryptoProviderId(cpc.Id);

            MemoryStream ms = new MemoryStream(data);
            MemoryStream ms2 = new MemoryStream(Resources.SKWithRemoveHashAndDBP);
            MemoryStream ms3 = new MemoryStream();

            var header = Header.ReadHeader<Header>(ms2);
            Assert.Equal(cpc, header.Configuration);

            Kes kes = new Kes(ks, header.Configuration);
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
            cpc.Id = CryptoProviderFactory.GetRegisteredCryptoProviderId(cpc.Id);

            MemoryStream ms = new MemoryStream(data);
            MemoryStream ms2 = new MemoryStream(Resources.MVWithRemoveHashAndDBP);
            MemoryStream ms3 = new MemoryStream();

            var header = Header.ReadHeader<Header>(ms2);
            Assert.Equal(cpc, header.Configuration);

            Kes kes = new Kes(ks, header.Configuration);
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
            cpc.Id = CryptoProviderFactory.GetRegisteredCryptoProviderId(cpc.Id);

            MemoryStream ms = new MemoryStream(data);
            MemoryStream ms2 = new MemoryStream(Resources.DEWithRemoveHashAndDBP);
            MemoryStream ms3 = new MemoryStream();

            var header = Header.ReadHeader<Header>(ms2);
            Assert.Equal(cpc, header.Configuration);

            Kes kes = new Kes(ks, header.Configuration);
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
            cpc.Id = CryptoProviderFactory.GetRegisteredCryptoProviderId(cpc.Id);

            MemoryStream ms = new MemoryStream(data);
            MemoryStream ms2 = new MemoryStream(Resources.SKWithContinuousAndRemoveHashAndDBP);
            MemoryStream ms3 = new MemoryStream();

            var header = Header.ReadHeader<Header>(ms2);
            Assert.Equal(cpc, header.Configuration);

            Kes kes = new Kes(ks, header.Configuration);
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
            cpc.Id = CryptoProviderFactory.GetRegisteredCryptoProviderId(cpc.Id);

            MemoryStream ms = new MemoryStream(data);
            MemoryStream ms2 = new MemoryStream(Resources.MVWithContinuousAndRemoveHashAndDBP);
            MemoryStream ms3 = new MemoryStream();

            var header = Header.ReadHeader<Header>(ms2);
            Assert.Equal(cpc, header.Configuration);

            Kes kes = new Kes(ks, header.Configuration);
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
            cpc.Id = CryptoProviderFactory.GetRegisteredCryptoProviderId(cpc.Id);

            MemoryStream ms = new MemoryStream(data);
            MemoryStream ms2 = new MemoryStream(Resources.DEWithContinuousAndRemoveHashAndDBP);
            MemoryStream ms3 = new MemoryStream();

            var header = Header.ReadHeader<Header>(ms2);
            Assert.Equal(cpc, header.Configuration);

            Kes kes = new Kes(ks, header.Configuration);
            kes.DecryptAsync(ms2, ms3).Wait();

            Assert.Equal(ms.ToArray(), ms3.ToArray());
        }
    }
}
