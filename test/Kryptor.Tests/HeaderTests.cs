using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPTeam.Kryptor.Tests
{
    public class HeaderTests
    {
        [Fact]
        public void HeaderFunctionalityTest()
        {
            var ex = new Dictionary<string, string>
            {
                ["test"] = "1",
                ["2"] = "test"
            };

            Header header = new Header()
            {
                Version = new Version(1, 1),
                EngineVersion = new Version(2, 2),
                Verbosity = HeaderVerbosity.Maximum,
                BlockSize = 5640,
                Configuration = new CryptoProviderConfiguration()
                {
                    Id = "MV",
                    Continuous = true,
                    RemoveHash = false,
                    DynamicBlockProccessing = true,
                },
                Extra = ex,
            };

            var mem = new MemoryStream(header.CreatePayload());

            Header h2 = Header.ReadHeader<Header>(mem);

            Assert.NotNull(h2);
            Assert.Equal(header.Version, h2.Version);
            Assert.Equal(header.EngineVersion, h2.EngineVersion);
            Assert.Equal(header.Verbosity, h2.Verbosity);
            Assert.Equal(header.BlockSize, h2.BlockSize);
            Assert.Equal(header.Configuration.Id, h2.Configuration.Id);
            Assert.Equal(header.Configuration.Continuous, h2.Configuration.Continuous);
            Assert.Equal(header.Configuration.RemoveHash, h2.Configuration.RemoveHash);
            Assert.Equal(header.Configuration.DynamicBlockProccessing, h2.Configuration.DynamicBlockProccessing);
            Assert.Equal(header.Extra, h2.Extra);

            Kes kp = new Kes(KeyStore.Generate(), header.Configuration, header.BlockSize);

            var mem2 = new MemoryStream(Encoding.UTF8.GetBytes("test"));
            var mem3 = new MemoryStream();

            var ex2 = new Dictionary<string, string>
            {
                ["test"] = "1",
                ["2"] = "test"
            };

            Header header2 = new Header()
            {
                Verbosity = HeaderVerbosity.Maximum,
                Extra = ex2,
            };

            kp.EncryptAsync(mem2, mem3, header2).Wait();

            Header h3 = Header.ReadHeader<Header>(mem3);

            Assert.NotNull(h3);
            Assert.Equal(header.Verbosity, h3.Verbosity);
            Assert.Equal(header.BlockSize, h3.BlockSize);
            Assert.NotEqual(header.Configuration.Id, h3.Configuration.Id);
            Assert.Equal("kryptor:MixedVector", h3.Configuration.Id);
            Assert.Equal(header.Configuration.Continuous, h3.Configuration.Continuous);
            Assert.Equal(header.Configuration.RemoveHash, h3.Configuration.RemoveHash);
            Assert.Equal(header.Configuration.DynamicBlockProccessing, h3.Configuration.DynamicBlockProccessing);
            Assert.Equal(header.Extra, h3.Extra);
        }
    }
}
