using System.Text;

using SAPTeam.Kryptor.CryptoProviders;

namespace SAPTeam.Kryptor.Tests
{
    public class KesTests
    {
        private string testText = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789/*-+=;";
        private byte[] testBytes = new byte[16] { 53, 15, 79, 254, 74, 156, 59, 88, 1, 0, 255, 65, 198, 36, 59, 214 };

        [Fact]
        public async void EncryptDecryptTest()
        {
            KeyStore ks = KeyStore.Generate(128);
            Kes kp = new Kes(new StandaloneKey(ks));

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
            Kes kp = new Kes(new StandaloneKey(ks), blockSize: 0x8000);

            Assert.Equal(1048576, kp.DecryptionBufferSize);
            Assert.Equal(((1048576 / 32) - 1) * 31, kp.EncryptionBufferSize);
        }

        [Fact]
        public async void InvalidKeystoreTest()
        {
            KeyStore ks = KeyStore.Generate(128);
            Kes kp = new Kes(new StandaloneKey(ks));

            KeyStore ks2 = KeyStore.Generate(128);
            Kes kp2 = new Kes(new StandaloneKey(ks2));

            byte[] enc = await kp.EncryptBlockAsync(testBytes);
            await Assert.ThrowsAsync<InvalidDataException>(async () => await kp2.DecryptBlockAsync(enc));
        }

        [Fact]
        public async void EncryptOverflow()
        {
            KeyStore ks = KeyStore.Generate(128);
            Kes kp = new Kes(new StandaloneKey(ks), blockSize: 0x8000);

            byte[] buffer = new byte[kp.EncryptionBufferSize + 1];
            Random.Shared.NextBytes(buffer);
            await Assert.ThrowsAsync<ArgumentException>(async () => await kp.EncryptBlockAsync(buffer));
        }

        [Fact]
        public async void DecryptOverflow()
        {
            KeyStore ks = KeyStore.Generate(128);
            Kes kp = new Kes(new StandaloneKey(ks), blockSize: 0x8000);

            byte[] buffer = new byte[kp.DecryptionBufferSize + 1];
            Random.Shared.NextBytes(buffer);
            await Assert.ThrowsAsync<ArgumentException>(async () => await kp.DecryptBlockAsync(buffer));
        }
    }
}
