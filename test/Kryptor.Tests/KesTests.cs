using System.Text;

namespace SAPTeam.Kryptor.Tests
{
    public class KesTests
    {
        private readonly string testText = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789/*-+=;";
        private readonly byte[] testBytes = new byte[16] { 53, 15, 79, 254, 74, 156, 59, 88, 1, 0, 255, 65, 198, 36, 59, 214 };

        [Fact]
        public async void EncryptDecryptTest()
        {
            KeyStore ks = KeyStore.Generate(128);
            CryptoProvider cp = CryptoProviderFactory.Create(ks, "MixedVector");
            Kes kp = new Kes(cp);

            byte[] enc = await kp.EncryptBlockAsync(Encoding.UTF8.GetBytes(testText));
            byte[] output = await kp.DecryptBlockAsync(enc);
            Assert.Equal(testText, Encoding.UTF8.GetString(output));

            byte[] enc2 = await kp.EncryptBlockAsync(testBytes);
            byte[] output2 = await kp.DecryptBlockAsync(enc2);
            Assert.Equal(testBytes, output2);
        }

        [Fact]
        public void BlockSizeTest()
        {
            KeyStore ks = KeyStore.Generate(128);
            CryptoProvider cp = CryptoProviderFactory.Create(ks, "MixedVector");
            Kes kp = new Kes(cp, blockSize: 0x8000);

            Assert.Equal(16777216, kp.GetDecryptionBufferSize());
            Assert.Equal((16777216 / 512 * 511) - 32, kp.GetEncryptionBufferSize());
        }

        [Fact]
        public async void InvalidKeystoreTest()
        {
            KeyStore ks = KeyStore.Generate(128);
            CryptoProvider cp = CryptoProviderFactory.Create(ks, "MixedVector");
            Kes kp = new Kes(cp);

            KeyStore ks2 = KeyStore.Generate(128);
            CryptoProvider cp2 = CryptoProviderFactory.Create(ks2, "MixedVector");
            Kes kp2 = new Kes(cp2);

            byte[] enc = await kp.EncryptBlockAsync(testBytes);
            await Assert.ThrowsAsync<InvalidDataException>(async () => await kp2.DecryptBlockAsync(enc));
        }

        [Fact]
        public async void EncryptOverflow()
        {
            KeyStore ks = KeyStore.Generate(128);
            CryptoProvider cp = CryptoProviderFactory.Create(ks, "MixedVector");
            Kes kp = new Kes(cp, blockSize: 0x8000);

            byte[] buffer = new byte[kp.GetEncryptionBufferSize() + 1];
            Random.Shared.NextBytes(buffer);
            await Assert.ThrowsAsync<ArgumentException>(async () => await kp.EncryptBlockAsync(buffer));
        }

        [Fact]
        public async void DecryptOverflow()
        {
            KeyStore ks = KeyStore.Generate(128);
            CryptoProvider cp = CryptoProviderFactory.Create(ks, "MixedVector");
            Kes kp = new Kes(cp, blockSize: 0x8000);

            byte[] buffer = new byte[kp.GetDecryptionBufferSize() + 1];
            Random.Shared.NextBytes(buffer);
            await Assert.ThrowsAsync<ArgumentException>(async () => await kp.DecryptBlockAsync(buffer));
        }
    }
}
