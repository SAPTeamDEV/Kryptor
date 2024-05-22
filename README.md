Outdated Readme
---------------
This Readme file belongs to the first alpha versions of Kryptor, The public API of this library heavily changed and this file will be updated on first stable release.

Kryptor
=========================================

[![Gawe CI](https://github.com/SAPTeamDEV/Kryptor/actions/workflows/main.yml/badge.svg?event=push)](https://github.com/SAPTeamDEV/Kryptor/actions/workflows/main.yml)
[![CodeQL](https://github.com/SAPTeamDEV/Kryptor/actions/workflows/codeql.yml/badge.svg?event=push)](https://github.com/SAPTeamDEV/Kryptor/actions/workflows/codeql.yml)
[![codecov](https://codecov.io/gh/SAPTeamDEV/Kryptor/branch/master/graph/badge.svg)](https://codecov.io/gh/SAPTeamDEV/Kryptor)
[![NuGet](https://img.shields.io/nuget/v/SAPTeam.Kryptor)](https://www.nuget.org/packages/SAPTeam.Kryptor)
[![NuGet](https://img.shields.io/nuget/dt/SAPTeam.Kryptor)](https://www.nuget.org/packages/SAPTeam.Kryptor)

Kryptor is a **key-based encryption engine** that currently uses the **KES** encryption algorithm to encrypt and decrypt data.

What is the KES?
----------------
KES is a complex encryption algorithm that uses a keystore to encrypt and decrypt data.

A keystore can contain up to 128 keys, and each key is used to encrypt and decrypt parts of data.
Every 32 bytes of data is encrypted using a different key, and the keys are used in a round-robin fashion.
in other words, the first 32 bytes of data are encrypted using the first key, the next 32 bytes are encrypted using the second key, and so on.
When the end of the keystore is reached, the encryption process starts over from the beginning of the keystore.

In this way, in a 128-key keystore, the first 128*32 bytes of data are encrypted using the first 128 keys, and then the process starts over from the beginning of the keystore.

so in a 1 MB file, each key is used to encrypt and decrypt 8 KB of non-consecutive data.
that makes it very difficult for an attacker to decrypt the data without the keystore.
because the attacker would have to guess all 128 keys in the keystore to decrypt the data.

Each key has a length of 256 bits, so there are 2^256 possible options to guess, just for 1 key in the keystore, which makes it impossible for an attacker to guess all the keys.

How to use the KES?
-------------------
To use the KES, you need to create a keystore using the KES Keystore Generator, and then use the keystore to encrypt and decrypt data using the KESProvider.

1. Create a keystore using the KES Keystore Generator			

```c#
KESKeyStore ks = KESKeyStore.Generate();
```

2. Save the keystore to a file to use it later

```c#
File.WriteAllLines("keystore.kks", new string[] {ks.ToString()});
```

3. Encrypt data using the KESProvider
```c#
KESProvider kp = new KESProvider(ks);
kp.EncryptFile("InputFile", "OutputFile");
```

and to decrypt the data, use the same keystore to decrypt the data

1. Load the keystore from the file
```c#
KESKeyStore ks = KESKeyStore.FromString(File.ReadAllText("keystore.kks"));
```

2. Decrypt the data using the KESProvider
```c#
KESProvider kp = new KESProvider(ks);
kp.DecryptFile("InputFile", "OutputFile");
```

Note: You can also use the KESProvider to encrypt and decrypt data in memory with the EncryptBlock and DecryptBlock methods.
