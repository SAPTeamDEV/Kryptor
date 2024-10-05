using System.Text;

namespace SAPTeam.Kryptor.Tests
{
    public class HeaderTests
    {
        [Fact]
        public void HeaderFunctionalityTest()
        {
            Dictionary<string, string> ex = new Dictionary<string, string>
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
                    DynamicBlockProcessing = true,
                },
                Extra = ex,
            };

            MemoryStream mem = new MemoryStream(header.CreatePayload());

            Header h2 = Header.ReadHeader<Header>(mem);

            Assert.NotNull(h2);
            Assert.Equal(header.Version, h2.Version);
            Assert.Equal(header.EngineVersion, h2.EngineVersion);
            Assert.Equal(header.Verbosity, h2.Verbosity);
            Assert.Equal(header.BlockSize, h2.BlockSize);
            Assert.Equal(header.Configuration.Id, h2.Configuration.Id);
            Assert.Equal(header.Configuration.Continuous, h2.Configuration.Continuous);
            Assert.Equal(header.Configuration.RemoveHash, h2.Configuration.RemoveHash);
            Assert.Equal(header.Configuration.DynamicBlockProcessing, h2.Configuration.DynamicBlockProcessing);
            Assert.Equal(header.Extra, h2.Extra);

            Kes kp = new Kes(KeyStore.Generate(), header.Configuration, header.BlockSize);

            MemoryStream mem2 = new MemoryStream(Encoding.UTF8.GetBytes("test"));
            MemoryStream mem3 = new MemoryStream();

            Dictionary<string, string> ex2 = new Dictionary<string, string>
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
            Assert.Equal(header.Configuration.DynamicBlockProcessing, h3.Configuration.DynamicBlockProcessing);
            Assert.Equal(header.Extra, h3.Extra);
        }
    }
}
