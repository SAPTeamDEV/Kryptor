﻿using System;
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
        public async void EncryptDecryptTest()
        {
            KESKeyStore ks = KESKeyStore.Generate(128);
            KESProvider kp = new KESProvider(ks);

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
            KESKeyStore ks = KESKeyStore.Generate(128);
            KESProvider kp = new KESProvider(ks, 1048576);

            Assert.Equal(1048576, kp.DecryptionBlockSize);
            Assert.Equal((1048576 / 32 - 1) * 31, kp.EncryptionBlockSize);

            Assert.Throws<ArgumentException>(() => new KESProvider(ks, 127));
        }

        [Fact]
        public async void InvalidKeystoreTest()
        {
            KESKeyStore ks = KESKeyStore.Generate(128);
            KESProvider kp = new KESProvider(ks);

            KESKeyStore ks2 = KESKeyStore.Generate(128);
            KESProvider kp2 = new KESProvider(ks2);

            byte[] enc = await kp.EncryptBlockAsync(testBytes);
            await Assert.ThrowsAsync<InvalidDataException>(async () => await kp2.DecryptBlockAsync(enc));
        }

        [Fact]
        public async void EncryptOverflow()
        {
            KESKeyStore ks = KESKeyStore.Generate(128);
            KESProvider kp = new KESProvider(ks, 1048576);

            byte[] buffer = new byte[kp.EncryptionBlockSize + 1];
            Random.Shared.NextBytes(buffer);
            await Assert.ThrowsAsync<ArgumentException>(async () => await kp.EncryptBlockAsync(buffer));
        }

        [Fact]
        public async void DecryptOverflow()
        {
            KESKeyStore ks = KESKeyStore.Generate(128);
            KESProvider kp = new KESProvider(ks, 1048576);

            byte[] buffer = new byte[kp.DecryptionBlockSize + 1];
            Random.Shared.NextBytes(buffer);
            await Assert.ThrowsAsync<ArgumentException>(async () => await kp.DecryptBlockAsync(buffer));
        }
    }
}