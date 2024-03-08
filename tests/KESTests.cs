using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SAPTeam.Kryptor;

namespace Kryptor.Tests
{
    public class KESTests
    {
        string testText = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789/*-+=;";
        byte[] testBytes = new byte[16] { 53, 15, 79, 254, 74, 156, 59, 88, 1, 0, 255, 65, 198, 36, 59, 214 };

        [Fact]
        public void EncryptDecryptTest()
        {
            KESKeyStore ks = KESKeyStore.Generate(128);
            KESProvider kp = new KESProvider(ks);

            byte[] enc = kp.EncryptBlock(Encoding.UTF8.GetBytes(testText));
            byte[] output = kp.DecryptBlock(enc);
            Assert.Equal(testText, Encoding.UTF8.GetString(output));

            byte[] enc2 = kp.EncryptBlock(testBytes);
            byte[] output2 = kp.DecryptBlock(enc2);
            Assert.Equal(testBytes,output2);
        }

        [Fact]
        public void BlockSizeTest()
        {
            KESKeyStore ks = KESKeyStore.Generate(128);
            KESProvider kp = new KESProvider(ks, 1048576);

            Assert.Equal(1048576, kp.DecryptionBlockSize);
            Assert.Equal((1048576 / 32 - 1) * 31, kp.EncryptionBlockSize);

            Assert.Throws<ArgumentException>(() => new KESProvider(ks, 127));
        }

        [Fact]
        public void InvalidKeystoreTest()
        {
            KESKeyStore ks = KESKeyStore.Generate(128);
            KESProvider kp = new KESProvider(ks);

            KESKeyStore ks2 = KESKeyStore.Generate(128);
            KESProvider kp2 = new KESProvider(ks2);

            byte[] enc = kp.EncryptBlock(testBytes);
            Assert.Throws<InvalidDataException>(() => kp2.DecryptBlock(enc));
        }

        [Fact]
        public void EncryptOverflow()
        {
            KESKeyStore ks = KESKeyStore.Generate(128);
            KESProvider kp = new KESProvider(ks, 1048576);

            byte[] buffer = new byte[kp.EncryptionBlockSize + 1];
            Random.Shared.NextBytes(buffer);
            Assert.Throws<ArgumentException>(() => kp.EncryptBlock(buffer));
        }

        [Fact]
        public void DecryptOverflow()
        {
            KESKeyStore ks = KESKeyStore.Generate(128);
            KESProvider kp = new KESProvider(ks, 1048576);

            byte[] buffer = new byte[kp.DecryptionBlockSize + 1];
            Random.Shared.NextBytes(buffer);
            Assert.Throws<ArgumentException>(() => kp.DecryptBlock(buffer));
        }
    }
}
