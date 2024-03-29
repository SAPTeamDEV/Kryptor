using SAPTeam.Kryptor;

namespace Kryptor.Tests
{
    public class KeyStoreTests
    {
        [Fact]
        public void KeyGeneratorTest()
        {
            KESKeyStore ks = KESKeyStore.Generate(128);

            Assert.All<byte[]>(ks.Keys, x => Assert.Equal(32, x.Length));
        }

        [Fact]
        public void KeyConvertTest()
        {
            KESKeyStore ks = KESKeyStore.Generate(128);
            string b64Key = ks.ToString();
            KESKeyStore ks2 = KESKeyStore.FromString(b64Key);

            Assert.Equal(b64Key, ks2.ToString());
            Assert.Equal(ks.Keys, ks2.Keys);
        }

        [Fact]
        public void IndexHandlerTest()
        {
            KESKeyStore ks = KESKeyStore.Generate(128);

            // Some random key access
            Assert.Equal(ks[0], ks[128]);
            Assert.Equal(ks[1], ks[129]);
            Assert.Equal(ks[10], ks[138]);
            Assert.Equal(ks[50], ks[178]);
            Assert.Equal(ks[127], ks[255]);

            Assert.NotEqual(ks[0], ks[127]);
            Assert.NotEqual(ks[1], ks[128]);
            Assert.NotEqual(ks[10], ks[137]);
            Assert.NotEqual(ks[50], ks[177]);
            Assert.NotEqual(ks[127], ks[254]);
        }
    }
}