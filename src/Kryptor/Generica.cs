using System;
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
        public Generica(byte[] seed)
        {
            _sha256 = SHA256.Create();
            _sha384 = SHA384.Create();
            _sha512 = SHA512.Create();

            _seed = BitConverter.ToString(_sha512.ComputeHash(seed));
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
            byte[] tl = new byte[5]
            {
                (byte)(input.Length * 6 % 256),
                (byte)Math.Abs((input.Length - _sCount) % 256),
                (byte)(input.Length * 124 % 256),
                (byte)(input.Length * 75 % 128),
                (byte)(input.Length * 13 % 64),
            };

            tl = _sha384.ComputeHash(tl);

            byte[] hashes = new byte[3][]
            {
                _sha512.ComputeHash(Encode(MoreEnumerable.Repeat(ChangeCase(_seed), Math.Max(input.Length % 10, 1)).ToArray())),
                _sha384.ComputeHash(Encode(new string(_seed.Chunk(_sCount / 2).Last()).PadRight(_sCount * 2, Convert.ToString(_sha512.ComputeHash(tl))[5]).PadLeft(_sCount * 5, Convert.ToString(_sha384.ComputeHash(tl.Base64EncodeToByte()))[6]))),
                _sha256.ComputeHash(Encode(ChangeCase(Convert.ToBase64String(Encode(_seed)))))
            }.SelectMany(x => x).OrderBy(x => x * 9 % 24).ToArray();

            byte[] vm = _sha384.ComputeHash(hashes.Select(x => (byte)((x * 7) % 256)).ToArray());
            byte[] vf = _sha512.ComputeHash(hashes).Concat(_sha256.ComputeHash(ChangeCase(tl.Base64Encode()).Base64EncodeToByte()).Base64EncodeToByte()).ToArray();
            byte[] vt = _sha256.ComputeHash(vf.Concat(_sha384.ComputeHash(hashes.Select(x => (byte)(((x * 11 / 4 * 6) + 5) % 256)).ToArray())).ToArray());

            int i = 0;

            while(i < input.Length)
            {
                vt = _sha384.ComputeHash(_sha512.ComputeHash(vt)
                                                .Concat(_sha384.ComputeHash(new byte[] { (byte)(Math.Abs(input.Length - i) % 256) }))
                                                .Concat(vf.Take(Math.Abs(input.Length - i) % 64))
                                                .Concat(vm.Take((i + 2) * 3 % 48))
                                                .ToArray());

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
