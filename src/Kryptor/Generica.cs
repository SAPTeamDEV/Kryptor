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
        public Generica(string seed)
        {
            _seed = seed;
            _sCount = seed.Length;

            _sha256 = SHA256.Create();
            _sha384 = SHA384.Create();
            _sha512 = SHA512.Create();
        }

        /// <summary>
        /// Fills the input bytes array with generated values.
        /// </summary>
        /// <param name="input">
        /// The input buffer.
        /// </param>
        public void Generate(byte[] input)
        {
            byte[] v1 = _sha512.ComputeHash(Encode(MoreEnumerable.Repeat(ChangeCase(_seed), 5).ToArray()));
            byte[] v2 = _sha384.ComputeHash(Encode(new string(_seed.Chunk(_sCount / 2).First()).PadRight(32, '5').PadLeft(86, '9')));
            byte[] v3 = _sha256.ComputeHash(Encode(ChangeCase(Convert.ToBase64String(Encode(_seed)))));

            byte[] vf = _sha512.ComputeHash(v1.Concat(v2).Concat(v3).ToArray());
            byte[] vt = _sha256.ComputeHash(vf);

            for (int i = 0; i < input.Length; i++)
            {
                vt = _sha256.ComputeHash(vt);

                vt.CopyTo(input, i);

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
