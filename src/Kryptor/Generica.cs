﻿using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using MoreLinq;
using MoreLinq.Extensions;

namespace SAPTeam.Kryptor
{
    /// <summary>
    /// Generates bytes array populated from specified seed.
    /// </summary>
    public class Generica
    {
        readonly string _seed;
        readonly int _sCount;

        readonly SHA256 _sha256;
        readonly SHA384 _sha384;
        readonly SHA512 _sha512;

        /// <summary>
        /// Initializes a new instance of the <see cref="Generica"/> class.
        /// </summary>
        /// <param name="seed">
        /// The seed to generate values.
        /// </param>
        public Generica(string seed)
        {
            _sha256 = SHA256.Create();
            _sha384 = SHA384.Create();
            _sha512 = SHA512.Create();

            _seed = BitConverter.ToString(_sha512.ComputeHash(Encode(seed)));
            _sCount = _seed.Length;
        }

        /// <summary>
        /// Fills the input bytes array with generated values.
        /// </summary>
        /// <param name="input">
        /// The input buffer.
        /// </param>
        public void Generate(byte[] input)
        {
            byte[] hashes = new byte[3][]
            {
                _sha512.ComputeHash(Encode(MoreEnumerable.Repeat(ChangeCase(_seed), 5).ToArray())),
                _sha384.ComputeHash(Encode(new string(_seed.Chunk(_sCount / 2).Last()).PadRight(_sCount * 2, 't').PadLeft(_sCount * 5, 'Y'))),
                _sha256.ComputeHash(Encode(ChangeCase(Convert.ToBase64String(Encode(_seed)))))
            }.SelectMany(x => x).ToArray();

            byte[] vm = _sha384.ComputeHash(hashes.Select(x => (byte)((x * 7) % 256)).ToArray());
            byte[] vf = _sha512.ComputeHash(hashes);
            byte[] vt = _sha256.ComputeHash(vf.Concat(_sha384.ComputeHash(hashes.Select(x => (byte)((x * 11 / 4 * 6 + 5) % 256)).ToArray())).ToArray());

            int i = 0;

            while(i < input.Length)
            {
                vt = _sha384.ComputeHash(_sha512.ComputeHash(vt).Concat(vm.Take((i + 2) * 3 % 48)).ToArray());

                Array.Copy(vt, 0, input, i, Math.Min(vt.Length, input.Length - i));

                i += vt.Length;
            }
        }

        string ChangeCase(string src)
        {
            int c = 0;

            return new string(src.Select(x =>
            {
                if (c++ % 2 == 0)
                {
                    return x.ToString().ToUpper()[0];
                }
                else
                {
                    return x;
                }
            }).ToArray());
        }

        static byte[] Encode(char[] src)
        {
            return Encoding.UTF8.GetBytes(src);
        }

        static byte[] Encode(string src)
        {
            return Encoding.UTF8.GetBytes(src);
        }
    }
}